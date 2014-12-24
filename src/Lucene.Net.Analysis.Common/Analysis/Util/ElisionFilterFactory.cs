﻿using System.Collections.Generic;
using Lucene.Net.Analysis.Util;

namespace org.apache.lucene.analysis.util
{

	/*
	 * Licensed to the Apache Software Foundation (ASF) under one or more
	 * contributor license agreements.  See the NOTICE file distributed with
	 * this work for additional information regarding copyright ownership.
	 * The ASF licenses this file to You under the Apache License, Version 2.0
	 * (the "License"); you may not use this file except in compliance with
	 * the License.  You may obtain a copy of the License at
	 *
	 *     http://www.apache.org/licenses/LICENSE-2.0
	 *
	 * Unless required by applicable law or agreed to in writing, software
	 * distributed under the License is distributed on an "AS IS" BASIS,
	 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
	 * See the License for the specific language governing permissions and
	 * limitations under the License.
	 */


	using FrenchAnalyzer = org.apache.lucene.analysis.fr.FrenchAnalyzer;

	/// <summary>
	/// Factory for <seealso cref="ElisionFilter"/>.
	/// <pre class="prettyprint">
	/// &lt;fieldType name="text_elsn" class="solr.TextField" positionIncrementGap="100"&gt;
	///   &lt;analyzer&gt;
	///     &lt;tokenizer class="solr.StandardTokenizerFactory"/&gt;
	///     &lt;filter class="solr.LowerCaseFilterFactory"/&gt;
	///     &lt;filter class="solr.ElisionFilterFactory" 
	///       articles="stopwordarticles.txt" ignoreCase="true"/&gt;
	///   &lt;/analyzer&gt;
	/// &lt;/fieldType&gt;</pre>
	/// </summary>
	public class ElisionFilterFactory : TokenFilterFactory, ResourceLoaderAware, MultiTermAwareComponent
	{
	  private readonly string articlesFile;
	  private readonly bool ignoreCase;
	  private CharArraySet articles;

	  /// <summary>
	  /// Creates a new ElisionFilterFactory </summary>
	  public ElisionFilterFactory(IDictionary<string, string> args) : base(args)
	  {
		articlesFile = get(args, "articles");
		ignoreCase = getBoolean(args, "ignoreCase", false);
		if (args.Count > 0)
		{
		  throw new System.ArgumentException("Unknown parameters: " + args);
		}
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void inform(ResourceLoader loader) throws java.io.IOException
	  public virtual void inform(ResourceLoader loader)
	  {
		if (articlesFile == null)
		{
		  articles = FrenchAnalyzer.DEFAULT_ARTICLES;
		}
		else
		{
		  articles = getWordSet(loader, articlesFile, ignoreCase);
		}
	  }

	  public override ElisionFilter create(TokenStream input)
	  {
		return new ElisionFilter(input, articles);
	  }

	  public virtual AbstractAnalysisFactory MultiTermComponent
	  {
		  get
		  {
			return this;
		  }
	  }
	}


}