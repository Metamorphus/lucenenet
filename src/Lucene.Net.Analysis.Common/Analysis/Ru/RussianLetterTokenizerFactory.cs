﻿using Lucene.Net.Analysis.Util;
using Lucene.Net.Util;
using System;
using System.Collections.Generic;
using System.IO;

namespace Lucene.Net.Analysis.Ru
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

    /// @deprecated Use <seealso cref="org.apache.lucene.analysis.standard.StandardTokenizerFactory"/> instead.
    ///  This tokenizer has no Russian-specific functionality. 
    [Obsolete("Use StandardTokenizerFactory instead.")]
    public class RussianLetterTokenizerFactory : TokenizerFactory
    {

        /// <summary>
        /// Creates a new RussianLetterTokenizerFactory </summary>
        public RussianLetterTokenizerFactory(IDictionary<string, string> args) : base(args)
        {
            AssureMatchVersion();
            if (args.Count > 0)
            {
                throw new System.ArgumentException("Unknown parameters: " + args);
            }
        }

        public override Tokenizer Create(AttributeSource.AttributeFactory factory, TextReader input)
        {
            return new RussianLetterTokenizer(luceneMatchVersion, factory, input);
        }
    }
}