using System;

namespace PRoCon.Core.Logging
{
    public class LogEntry
    {

        public DateTime Logged
        {
            get;
            private set;
        }

        public string Text
        {
            get;
            private set;
        }

        public LogEntry(DateTime dtLogged, string strText)
        {
            this.Logged = dtLogged;
            this.Text = strText;
        }
    }
}
