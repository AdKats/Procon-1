using PRoCon.Core.Logging;
using PRoCon.Core.Plugin;
using PRoCon.Core.Remote;
using System;
using System.Collections.Generic;

namespace PRoCon.Core.Consoles
{
    public class PluginConsole : Loggable
    {
        protected PRoConClient Client;

        public PluginConsole(PRoConClient prcClient) : base()
        {
            Client = prcClient;

            LogEntries = new Queue<LogEntry>();

            FileHostNamePort = Client.FileHostNamePort;
            LoggingStartedPrefix = "Plugin logging started";
            LoggingStoppedPrefix = "Plugin logging stopped";
            FileNameSuffix = "plugin";

            Client.CompilingPlugins += new PRoConClient.EmptyParamterHandler(m_prcClient_CompilingPlugins);
            Client.RecompilingPlugins += new PRoConClient.EmptyParamterHandler(m_prcClient_RecompilingPlugins);
        }

        public Queue<LogEntry> LogEntries { get; private set; }
        public event WriteConsoleHandler WriteConsole;

        private void m_prcClient_RecompilingPlugins(PRoConClient sender)
        {
            Client.PluginsManager.PluginOutput += new PluginManager.PluginOutputHandler(Plugins_PluginOutput);
        }

        private void m_prcClient_CompilingPlugins(PRoConClient sender)
        {
            Client.PluginsManager.PluginOutput += new PluginManager.PluginOutputHandler(Plugins_PluginOutput);
        }

        private void Plugins_PluginOutput(string strOutput)
        {
            Write(strOutput);
        }

        public void Write(string strFormat, params string[] arguments)
        {
            try
            {
                DateTime dtLoggedTime = DateTime.UtcNow.ToUniversalTime().AddHours(Client.Game.UtcOffset).ToLocalTime();
                string strText = String.Format(strFormat, arguments);

                WriteLogLine(String.Format("[{0}] {1}", dtLoggedTime.ToString("HH:mm:ss"), strText));

                if (WriteConsole != null)
                {
                    this.WriteConsole(dtLoggedTime, strText);
                }

                LogEntries.Enqueue(new LogEntry(dtLoggedTime, strText));

                while (LogEntries.Count > 100)
                {
                    LogEntries.Dequeue();
                }
            }
            catch (Exception)
            {
            }
        }
    }
}