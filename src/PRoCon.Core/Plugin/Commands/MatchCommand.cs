// Copyright 2010 Geoffrey 'Phogue' Green
// 
// http://www.phogue.net
//  
// This file is part of PRoCon Frostbite.
//  
// PRoCon Frostbite is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// PRoCon Frostbite is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//  
// You should have received a copy of the GNU General Public License
// along with PRoCon Frostbite.  If not, see <http://www.gnu.org/licenses/>.


using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PRoCon.Core.Plugin.Commands
{

    [Serializable]
    public class MatchCommand
    {

        public string RegisteredClassname
        {
            get;
            private set;
        }

        public string RegisteredMethodName
        {
            get;
            private set;
        }

        public List<string> Scope
        {
            get;
            private set;
        }

        public string Command
        {
            get;
            private set;
        }

        public List<MatchArgumentFormat> ArgumentsFormat
        {
            get;
            private set;
        }

        public ExecutionRequirements Requirements
        {
            get;
            private set;
        }

        public string Description
        {
            get;
            private set;
        }

        /// <summary>
        /// Used for confirmation commands (commands that never get registered to procon).
        /// This may expand in the future to @cancel commands as well.
        /// </summary>
        /// <param name="lstScope"></param>
        /// <param name="strCommand"></param>
        /// <param name="lstArgumentsFormat"></param>
        public MatchCommand(List<string> lstScope, string strCommand, List<MatchArgumentFormat> lstArgumentsFormat)
        {
            this.RegisteredClassname = String.Empty;
            this.RegisteredMethodName = String.Empty;
            this.Scope = lstScope;
            this.Command = strCommand;
            this.ArgumentsFormat = lstArgumentsFormat;
            this.Requirements = null;
            this.Description = String.Empty;
        }

        /// <summary>
        /// Used for commands to be registered to procon.
        /// </summary>
        /// <param name="strRegisteredClassname"></param>
        /// <param name="strRegisteredMethodName"></param>
        /// <param name="lstScope"></param>
        /// <param name="strCommand"></param>
        /// <param name="lstArgumentsFormat"></param>
        /// <param name="erRequirements"></param>
        /// <param name="strDescription"></param>
        public MatchCommand(string strRegisteredClassname, string strRegisteredMethodName, List<string> lstScope, string strCommand, List<MatchArgumentFormat> lstArgumentsFormat, ExecutionRequirements erRequirements, string strDescription)
        {
            this.RegisteredClassname = strRegisteredClassname;
            this.RegisteredMethodName = strRegisteredMethodName;
            this.Scope = lstScope;
            this.Command = strCommand;
            this.ArgumentsFormat = lstArgumentsFormat;
            this.Requirements = erRequirements;
            this.Description = strDescription;
        }

        public CapturedCommand Matches(string strText)
        {
            CapturedCommand ccReturn = null;

            Match mtcCommandMatch = Regex.Match(strText, String.Format("^/?(?<scope>{0})(?<command>{1})[ ]?(?<arguments>.*)", String.Join("|", this.Scope.ToArray()), this.Command), RegexOptions.IgnoreCase);

            if (mtcCommandMatch.Success == true)
            {

                string strRemainderArguments = mtcCommandMatch.Groups["arguments"].Value;
                List<MatchArgument> lstMatchedArguments = new List<MatchArgument>();
                int skippedBlankArguments = 0;

                foreach (MatchArgumentFormat argument in this.ArgumentsFormat)
                {

                    if (argument.ArgumentValues != null && argument.ArgumentValues.Count > 0)
                    {

                        string strArgument = String.Empty;

                        if (argument.ArgumentType == MatchArgumentFormatTypes.Dictionary)
                        {
                            int iMatchScore = MatchCommand.GetClosestMatch(strRemainderArguments, argument.ArgumentValues, out strArgument, out strRemainderArguments);

                            if (iMatchScore != int.MaxValue)
                            {
                                lstMatchedArguments.Add(new MatchArgument(strArgument, iMatchScore));
                            }
                        }
                        else if (argument.ArgumentType == MatchArgumentFormatTypes.Regex)
                        {

                            foreach (string regexInput in argument.ArgumentValues)
                            {

                                Match argumentMatch = Regex.Match(strRemainderArguments, String.Format("^({0})", regexInput));

                                if (argumentMatch.Success == true && argumentMatch.Groups.Count >= 2)
                                {

                                    strRemainderArguments = strRemainderArguments.Substring(argumentMatch.Value.Length, strRemainderArguments.Length - argumentMatch.Value.Length);

                                    lstMatchedArguments.Add(new MatchArgument(argumentMatch.Groups[1].Value, 0));

                                    break;
                                }
                            }
                        }
                    }

                    if (argument.ArgumentValues.Count == 0)
                    {
                        skippedBlankArguments++;
                    }
                }

                if (lstMatchedArguments.Count == this.ArgumentsFormat.Count - skippedBlankArguments)
                {
                    ccReturn = new CapturedCommand(mtcCommandMatch.Groups["scope"].Value, this.Command, lstMatchedArguments, strRemainderArguments);
                }
            }

            return ccReturn;
        }

        // Thanks Dr. Levenshtein and Sam Allen @ http://dotnetperls.com/levenshtein
        private static int Compute(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            // Step 1
            if (n == 0)
            {
                return m;
            }

            if (m == 0)
            {
                return n;
            }

            // Step 2
            for (int i = 0; i <= n; d[i, 0] = i++)
            {
            }

            for (int j = 0; j <= m; d[0, j] = j++)
            {
            }

            // Step 3
            for (int i = 1; i <= n; i++)
            {
                //Step 4
                for (int j = 1; j <= m; j++)
                {
                    // Step 5
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                    // Step 6
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            // Step 7
            return d[n, m];
        }

        private static int GetClosestMatch(string strArguments, List<string> lstDictionary, out string strMatchedDictionaryKey, out string strRemainderArguments)
        {
            int iSimilarity = int.MaxValue;
            int iScore = 0;

            strRemainderArguments = String.Empty;
            strMatchedDictionaryKey = String.Empty;

            if (lstDictionary.Count >= 1)
            {

                int iLargestDictionaryKey = 0;

                // Build array of default matches from the dictionary to store a rank for each match.
                // (it's designed to work on smaller dictionaries with say.. 32 player names in it =)
                List<MatchDictionaryKey> lstMatches = new List<MatchDictionaryKey>();
                foreach (string strDictionaryKey in lstDictionary)
                {
                    lstMatches.Add(new MatchDictionaryKey(strDictionaryKey));

                    if (strDictionaryKey.Length > iLargestDictionaryKey)
                    {
                        iLargestDictionaryKey = strDictionaryKey.Length;
                    }
                }

                // Rank each match, find the remaining characters for a match (arguements)
                for (int x = 1; x <= Math.Min(strArguments.Length, iLargestDictionaryKey); x++)
                {
                    // Skip it if it's a space (a space breaks a name and moves onto arguement.
                    // but the space could also be included in the dictionarykey, which will be checked
                    // on the next loop.
                    if (x + 1 < strArguments.Length && strArguments[x] != ' ')
                        continue;

                    for (int i = 0; i < lstMatches.Count; i++)
                    {
                        iScore = MatchCommand.Compute(strArguments.Substring(0, x).ToLower(), lstMatches[i].LowerCaseMatchedText);

                        if (iScore < lstMatches[i].MatchedScore)
                        {
                            lstMatches[i].MatchedScore = iScore;
                            lstMatches[i].MatchedScoreCharacters = x;
                        }
                    }
                }

                // Sort the matches
                lstMatches.Sort();

                int iBestCharactersMatched = lstMatches[0].MatchedScoreCharacters;
                iSimilarity = lstMatches[0].MatchedScore;
                strMatchedDictionaryKey = lstMatches[0].MatchedText;

                // Now though we want to loop through from start to end and see if a subset of what we entered is found.
                // if so then this will find the highest ranked item with a subset of what was entered and favour that instead.
                string strBestCharsSubstringLower = strArguments.Substring(0, iBestCharactersMatched).ToLower();
                for (int i = 0; i < lstMatches.Count; i++)
                {
                    if (lstMatches[i].LowerCaseMatchedText.Contains(strBestCharsSubstringLower) == true)
                    {
                        iSimilarity = lstMatches[i].MatchedScore;
                        strMatchedDictionaryKey = lstMatches[i].MatchedText;
                        iBestCharactersMatched = lstMatches[i].MatchedScoreCharacters;

                        break;
                    }
                }

                if (iBestCharactersMatched < strArguments.Length)
                {
                    strRemainderArguments = strArguments.Substring(iBestCharactersMatched + 1);
                }
                else
                {
                    strRemainderArguments = strArguments.Substring(iBestCharactersMatched);
                }

            }

            return iSimilarity;
        }

        public override string ToString()
        {

            string strToString = this.Command;

            foreach (MatchArgumentFormat mafArgument in this.ArgumentsFormat)
            {
                strToString = String.Format("{0} [{1}] ", strToString, mafArgument.ArgumentName);
            }

            return strToString;
        }
    }
}
