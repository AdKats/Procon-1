namespace PRoCon.Core.Options
{
    public class StatsLinkNameUrl
    {

        public string LinkName
        {
            get;
            private set;
        }

        public string LinkUrl
        {
            get;
            private set;
        }

        public StatsLinkNameUrl(string strLinkName, string strLinkUrl)
        {
            this.LinkName = strLinkName;
            this.LinkUrl = strLinkUrl;
        }
    }
}
