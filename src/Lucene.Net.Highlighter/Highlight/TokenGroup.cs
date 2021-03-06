﻿/*
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
using System;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Tokenattributes;

namespace Lucene.Net.Search.Highlight
{
    public class TokenGroup
    {
        private static readonly int MAX_NUM_TOKENS_PER_GROUP = 50;

        internal Token[] tokens = new Token[MAX_NUM_TOKENS_PER_GROUP];
        internal float[] scores = new float[MAX_NUM_TOKENS_PER_GROUP];

        internal int MatchStartOffset { get; set; }
        internal int MatchEndOffset { get; set; }

        public int NumTokens { get; internal set; } = 0;
        public int StartOffset { get; internal set; } = 0;
        public int EndOffset { get; private set; } = 0;
        public float TotalScore { get; private set; }

        private IOffsetAttribute offsetAtt;
        private ICharTermAttribute termAtt;

        public TokenGroup(TokenStream tokenStream)
        {
            offsetAtt = tokenStream.AddAttribute<IOffsetAttribute>();
            termAtt = tokenStream.AddAttribute<ICharTermAttribute>();
        }

        internal void AddToken(float score)
        {
            if (NumTokens < MAX_NUM_TOKENS_PER_GROUP)
            {
                int termStartOffset = offsetAtt.StartOffset();
                int termEndOffset = offsetAtt.EndOffset();
                if (NumTokens == 0)
                {
                    StartOffset = MatchStartOffset = termStartOffset;
                    EndOffset = MatchEndOffset = termEndOffset;
                    TotalScore += score;
                }
                else
                {
                    StartOffset = Math.Min(StartOffset, termStartOffset);
                    EndOffset = Math.Max(EndOffset, termEndOffset);
                    if (score > 0)
                    {
                        if (TotalScore == 0)
                        {
                            MatchStartOffset = termStartOffset;
                            MatchEndOffset = termEndOffset;
                        }
                        else
                        {
                            MatchStartOffset = Math.Min(MatchStartOffset, termStartOffset);
                            MatchEndOffset = Math.Max(MatchEndOffset, termEndOffset);
                        }
                        TotalScore += score;
                    }
                }
                Token token = new Token(termStartOffset, termEndOffset);
                token.SetEmpty().Append(termAtt);
                tokens[NumTokens] = token;
                scores[NumTokens] = score;
                NumTokens++;
            }
        }

        internal bool IsDistinct()
        {
            return offsetAtt.StartOffset() >= EndOffset;
        }

        internal void Clear()
        {
            NumTokens = 0;
            TotalScore = 0;
        }

        /// <summary>
        /// the "n"th token
        /// </summary>
        /// <param name="index">a value between 0 and numTokens -1</param>
        public Token GetToken(int index)
        {
            return tokens[index];
        }

        /// <summary>
        /// the "n"th score
        /// </summary>
        /// <param name="index">a value between 0 and numTokens -1</param>
        public float GetScore(int index)
        {
            return scores[index];
        }
    }
}
