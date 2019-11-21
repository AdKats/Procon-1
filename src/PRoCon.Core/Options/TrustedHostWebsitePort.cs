using System;

namespace PRoCon.Core.Options
{
    public class TrustedHostWebsitePort
    {

        public string HostWebsite
        {
            get;
            private set;
        }

        public UInt16 Port
        {
            get;
            private set;
        }

        public TrustedHostWebsitePort(string strHostWebsite, UInt16 ui16Port)
        {
            this.HostWebsite = strHostWebsite;
            this.Port = ui16Port;
        }
    }
}
