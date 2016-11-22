<#
.SYNOPSIS
    Builds, runs, packages and uploads packages for Lucenen.NET's .NET Core libraries

.PARAMETER NuGetExe
    Path to nuget.exe. Required for creating and uploading NuGet packages
.PARAMETER NuGetSource
    URI to upload NuGet packages to. Required for uploading NuGet packages
.PARAMETER NuGetApiKey
    API Key used to upload package to NuGet source.  Required for uploading NuGet packages

.PARAMETER CreatePackage
    Create NuGet packages
.PARAMETER UploadPackage
    Upload NuGet packages
.PARAMETER RunTests
    Run all test libraries

.PARAMETER Configuration
    Runs scripts with either Debug or Release configuration

.PARAMETER ExcludeTestCategories
    An array of test categories to exclude in test runs. Default is LongRunningTest
.PARAMETER FrameworksToTest
    An array of frameworks to run tests against. Default is "net451" and "netcoreapp1.0"

.PARAMETER Quiet
    Silence output.  Useful for piping Test output into a log file instead of to console.

.PARAMETER TestResultsDirectory
    Directory for NUnit TestResults.  Default is $PSScriptRoot\TestResults
.PARAMETER NuGetPackageDirectory
    Directory for generated NuGet packages.  Default is $PSScriptRoot\NuGetPackages

.EXAMPLE
    Build.ps1 -Configuration "Debug" -RunTests -Quiet

    Build all .NET Core projects as Debug and run all tests. Tests are run
    against "net451" and "netcoreapp1.0" frameworks and excludes
    "LongRunningTests".  All output for tests is piped into an output.log and
    then placed in the $TestResultsDirectory.
.EXAMPLE
    Build.ps1 C:\tools\nuget.exe -CreatePackage

    Creates NuGet packages for .NET Core projects compiled as Release.
.EXAMPLE
    Build.ps1 C:\tools\nuget.exe "http://myget.org/conniey/F/lucenenet-feed" "0000-0000-0000"

    Creates and uploads NuGet packages for .NET Core projects compiled as
    Release. Uploads projects to "http://myget.org/conniey/F/lucenenet-feed".
.EXAMPLE
    Build.ps1 -RunTests -ExcludeTestCategories @("DtdProcessingTest", "LongRunningTest") -FrameworksToTest @("netcoreapp1.0")

    Build all .NET Core projects as Release and run all tests. Tests are run
    against "netcoreapp1.0" frameworks and excludes "DtdProcessingTest" and
    "LongRunningTests".
#>

[CmdletBinding(DefaultParameterSetName="Default")]
param(
    [Parameter(Mandatory = $true, Position = 0, ParameterSetName="CreatePackage")]
    [Parameter(Mandatory = $true, Position = 0, ParameterSetName="UploadPackage")]
    [string]$NuGetExe,

    [Parameter(Mandatory = $true, Position = 1, ParameterSetName="UploadPackage")]
    [string]$NuGetSource,

    [Parameter(Mandatory = $true, Position = 2, ParameterSetName="UploadPackage")]
    [string]$NuGetApiKey,

    [Parameter(Mandatory = $true, ParameterSetName="CreatePackage")]
    [switch]$CreatePackage,
    [Parameter(Mandatory = $true, ParameterSetName="UploadPackage")]
    [switch]$UploadPackage,
    
    [switch]$RunTests,
    
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    
    [string[]]$ExcludeTestCategories = @("LongRunningTest", "DtdProcessingTest"),
    [string[]]$FrameworksToTest = @("net451", "netcoreapp1.0"),
    
    [switch]$Quiet,
    [string]$TestResultsDirectory,
    [string]$NuGetPackageDirectory
)

$root = $PSScriptRoot
$defaultNugetPackageDirectory = Join-Path $root "NuGetPackages"
$defaultTestResultsDirectory = Join-Path $root "TestResults"

function Compile-Projects($projects) {

    foreach ($project in $projects) {
        pushd $project.DirectoryName

        & dotnet.exe build --configuration $Configuration

        popd
    }
}

function Test-Projects($projects) {
    if ([string]::IsNullOrEmpty($TestResultsDirectory)) {
        $TestResultsDirectory = $defaultTestResultsDirectory
    }

    if (Test-Path $TestResultsDirectory) {
        Write-Host "Removing old test results..."

        Remove-Item $TestResultsDirectory -Recurse -Force
    }

    New-Item $TestResultsDirectory -ItemType Directory
    
    # Setting the preference so that we can run all the tests regardless of
    # errors that may happen.
    $oldPreference = $ErrorActionPreference
    $ErrorActionPreference = "Continue"

    # Generate the string to exclude categories from being tested
    $categoryString = ""
    
    if ($ExcludeTestCategories.Count -gt 0) {
        foreach ($category in $ExcludeTestCategories) {
            $formatted = [String]::Format("Category!={0}", $category);

            if ([string]::IsNullOrEmpty($categoryString)) {
                $categoryString = "--where=""$formatted"
            } else {
                $categoryString += " && $formatted"
            }
        }

        $categoryString += '"'
    }

    Write-Host "Categories to Ignore [$categoryString]"
    
    foreach ($project in $projects) {
        
        pushd $project.DirectoryName

        $testName = $project.Directory.Name
        $testFolder = Join-Path $TestResultsDirectory $testName

        New-Item $testFolder -ItemType Directory

        foreach ($framework in $FrameworksToTest) {
            Write-Host "Testing [$testName]..."
            
            $outputLog = "$framework.output.log"
            $testResult = "$framework.TestResult.xml"

            if ($Quiet) {
                & dotnet.exe test --configuration $Configuration --framework $framework $categoryString | Set-Content $outputLog
            } else {
                & dotnet.exe test --configuration $Configuration --framework $framework $categoryString
            }
        }

        popd
    }

    $ErrorActionPreference = $oldPreference
}

function Create-NuGetPackages($projects) {

    if ([string]::IsNullOrEmpty($NuGetPackageDirectory)) {
        $NuGetPackageDirectory = $defaultNugetPackageDirectory
    }

    if (!(Test-Path $NuGetPackageDirectory)) {
        New-Item $NuGetPackageDirectory -ItemType Directory
    }
    
    foreach ($project in $projects) {
        pushd $project.DirectoryName

        & dotnet.exe pack --configuration $Configuration --output $NuGetPackageDirectory

        popd
    }
}

function Upload-NuGetPackages {

    $packagesToUpload = Get-ChildItem $NuGetPackageDirectory | ? { $_.Extension.Equals(".nupkg") -and !$_.BaseName.Contains(".symbols") }

    foreach ($package in $packagesToUpload) {
        & $NuGetExe push $package.FullName -Source $NuGetSource -ApiKey $NuGetApiKey
    }
}

& where.exe dotnet.exe

if ($LASTEXITCODE -ne 0) {
    Write-Error "Could not find .NET CLI in PATH. Please install it."
}

# Stopping script if any errors occur
$ErrorActionPreference = "Stop"

& dotnet.exe restore

$projectJsons = Get-ChildItem $root\project.json -Recurse

Compile-Projects $projectJsons

if ($RunTests) {
    $testProjects = $projectJsons | ? { $_.Directory.Name.Contains(".Tests") }
    Test-Projects $testProjects
}

if ($CreatePackage -or $UploadPackage) {
    $projectsToPackage = $projectJsons | ? { !$_.Directory.Name.Contains(".Tests") }
    Create-NuGetPackages $projectsToPackage
}

if ($UploadPackage) {
    Upload-NuGetPackages
}