using System;

namespace PRoCon.Core.Plugin.Commands
{
    public class MatchDictionaryKey : IComparable<MatchDictionaryKey>
    {

        public MatchDictionaryKey(string strMatchedText)
        {
            this.MatchedText = strMatchedText;
            this.MatchedScoreCharacters = 0;
            this.MatchedScore = int.MaxValue;
        }

        public int CompareTo(MatchDictionaryKey other)
        {
            return MatchedScore.CompareTo(other.MatchedScore);
        }

        public string MatchedText
        {
            get;
            private set;
        }

        public string LowerCaseMatchedText
        {
            get
            {
                return this.MatchedText.ToLower();
            }
        }

        public int MatchedScoreCharacters
        {
            get;
            set;
        }

        public int MatchedScore
        {
            get;
            set;
        }
    }
}
