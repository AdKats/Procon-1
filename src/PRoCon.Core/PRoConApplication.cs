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

using MaxMind;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;

namespace PRoCon.Core
{
    using Core.Accounts;
    using Core.AutoUpdates;
    using Core.Battlemap;
    using Core.Events;
    using Core.HttpServer;
    using Core.Localization;
    using Core.Options;
    using Core.Players.Items;
    using Core.Remote;
    using Core.Variables;
    using Microsoft.Win32;
    // This, renamed or whatever, will eventually be the core app.
    // Will contain what frmMain.cs contains/does at the moment.

    public class PRoConApplication
    {

        public delegate void CurrentLanguageHandler(CLocalization language);
        public event CurrentLanguageHandler CurrentLanguageChanged;

        public delegate void ShowNotificationHandler(int timeout, string title, string text, bool isError);
        public event ShowNotificationHandler ShowNotification;

        public event HttpWebServer.StateChangeHandler HttpServerOnline;
        public event HttpWebServer.StateChangeHandler HttpServerOffline;

        public delegate void EmptyParameterHandler(PRoConApplication instance);
        public event EmptyParameterHandler BeginRssUpdate;
        public event EmptyParameterHandler RssUpdateError;

        public delegate void RssHandler(PRoConApplication instance, XmlDocument rss);
        public event RssHandler RssUpdateSuccess;

        public event EmptyParameterHandler BeginPromoUpdate;
        public event EmptyParameterHandler PromoUpdateError;
        public event RssHandler PromoUpdateSuccess;

        private CountryLookup m_clIpToCountry;

        public bool ConsoleMode { get; set; }

        public AccountDictionary AccountsList
        {
            get;
            private set;
        }

        public LocalizationDictionary Languages
        {
            get;
            private set;
        }

        public HttpWebServer HttpWebServer
        {
            get;
            private set;
        }

        private CLocalization m_clocCurrentLanguage;
        public CLocalization CurrentLanguage
        {
            get
            {
                return this.m_clocCurrentLanguage;
            }
            set
            {
                if (value != null)
                {
                    this.m_clocCurrentLanguage = value;

                    if (this.CurrentLanguageChanged != null)
                    {
                        this.CurrentLanguageChanged(value);
                    }

                    this.SaveMainConfig();
                }
            }
        }

        public ConnectionDictionary Connections
        {
            get;
            private set;
        }

        public OptionsSettings OptionsSettings
        {
            get;
            private set;
        }

        public bool LoadingAccountsFile
        {
            get;
            private set;
        }

        public bool LoadingMainConfig
        {
            get;
            set;
        }

        public AutoUpdater AutoUpdater
        {
            get;
            private set;
        }

        public string CustomTitle
        {
            get;
            private set;
        }

        public int MaxGspServers
        {
            get;
            private set;
        }

        public bool BlockUpdateChecks
        {
            get;
            private set;
        }

        public System.Windows.Forms.FormWindowState SavedWindowState
        {
            get;
            set;
        }

        public Rectangle SavedWindowBounds
        {
            get;
            set;
        }

        public XmlDocument ProconXml
        {
            get;
            private set;
        }

        public string LicenseKey
        {
            get;
            private set;
        }

        public List<string> LicenseAgreements
        {
            get;
            private set;
        }

        private int m_praPluginMaxRuntime_s;
        public int praPluginMaxRuntime_s
        {
            get
            {
                return this.m_praPluginMaxRuntime_s;
            }
            private set
            {
                if (value <= 0) { value = 10; }
                if (value >= 60) { value = 59; }
                this.m_praPluginMaxRuntime_s = value;
            }
        }

        private int m_praPluginMaxRuntime_m;
        public int praPluginMaxRuntime_m
        {
            get
            {
                return this.m_praPluginMaxRuntime_m;
            }
            private set
            {
                if (value < 0) { value = 0; }
                if (value >= 60) { value = 59; }
                this.m_praPluginMaxRuntime_m = value;
            }
        }

        private bool m_praIsPluginMaxRuntimeLocked;
        public bool praPluginMaxRuntimeLocked
        {
            get
            {
                return m_praIsPluginMaxRuntimeLocked;
            }
            private set
            {
                this.m_praIsPluginMaxRuntimeLocked = value;
            }
        }

        #region Regex

        // Moved here in 0.6.0.0 because each connection would compile these and they took up
        // a surprising amount of memory once combined.
        public Regex RegexMatchPunkbusterPlist { get; private set; }
        public Regex RegexMatchPunkbusterGuidComputed { get; private set; }
        public Regex RegexMatchPunkbusterBanlist { get; private set; }
        public Regex RegexMatchPunkbusterUnban { get; private set; }
        public Regex RegexMatchPunkbusterBanAdded { get; private set; }
        public Regex RegexMatchPunkbusterKickBanCmd { get; private set; }

        public Regex RegexMatchPunkbusterBeginPlist { get; private set; }
        public Regex RegexMatchPunkbusterEndPlist { get; private set; }

        #endregion

        //private Thread m_thChecker;
        protected Timer Checker { get; set; }

        private void GetGspSettings()
        {

            bool isEnabled = true;
            int iValue = int.MaxValue;

            if (File.Exists("PRoCon.xml") == true)
            {

                this.ProconXml = new XmlDocument();
                this.ProconXml.Load("PRoCon.xml");

                XmlNodeList OptionsList = this.ProconXml.GetElementsByTagName("options");
                if (OptionsList.Count > 0)
                {
                    XmlNodeList BlockUpdateChecksList = ((XmlElement)OptionsList[0]).GetElementsByTagName("blockupdatechecks");
                    if (BlockUpdateChecksList.Count > 0)
                    {
                        if (bool.TryParse(BlockUpdateChecksList[0].InnerText, out isEnabled) == true)
                        {
                            this.BlockUpdateChecks = isEnabled;
                        }
                    }

                    XmlNodeList NameList = ((XmlElement)OptionsList[0]).GetElementsByTagName("name");
                    if (NameList.Count > 0)
                    {
                        this.CustomTitle = NameList[0].InnerText;
                    }

                    XmlNodeList MaxServersList = ((XmlElement)OptionsList[0]).GetElementsByTagName("maxservers");
                    if (MaxServersList.Count > 0)
                    {
                        if (int.TryParse(MaxServersList[0].InnerText, out iValue) == true)
                        {
                            this.MaxGspServers = iValue;
                        }
                    }

                    XmlNodeList LicenseKeyList = ((XmlElement)OptionsList[0]).GetElementsByTagName("licensekey");
                    if (LicenseKeyList.Count > 0)
                    {
                        this.LicenseKey = LicenseKeyList[0].InnerText;
                    }

                    XmlNodeList LicenseList = ((XmlElement)OptionsList[0]).GetElementsByTagName("licenses");
                    if (LicenseList.Count > 0)
                    {

                        XmlNodeList LicenseAgrreementList = ((XmlElement)LicenseList[0]).GetElementsByTagName("agreement");
                        if (LicenseAgrreementList.Count > 0)
                        {
                            foreach (XmlNode licenseNode in LicenseAgrreementList)
                            {
                                this.LicenseAgreements.Add(licenseNode.InnerText);
                            }
                        }
                    }

                    XmlNodeList PluginRuntimeList = ((XmlElement)OptionsList[0]).GetElementsByTagName("pluginmaxruntime");
                    if (PluginRuntimeList.Count > 0)
                    {

                        XmlNodeList PluginRuntimeMList = ((XmlElement)PluginRuntimeList[0]).GetElementsByTagName("minutes");
                        if (PluginRuntimeMList.Count > 0)
                        {
                            int itmp;
                            if (int.TryParse(PluginRuntimeMList[0].InnerText, out itmp) == true)
                            {
                                this.praPluginMaxRuntime_m = itmp;
                                this.praPluginMaxRuntimeLocked = true;
                            }
                        }

                        XmlNodeList PluginRuntimeSList = ((XmlElement)PluginRuntimeList[0]).GetElementsByTagName("seconds");
                        if (PluginRuntimeSList.Count > 0)
                        {
                            int itmp;
                            if (int.TryParse(PluginRuntimeSList[0].InnerText, out itmp) == true)
                            {
                                this.praPluginMaxRuntime_s = itmp;
                                this.praPluginMaxRuntimeLocked = true;
                            }
                        }
                    }
                }
            }
        }

        public void SaveGspSettings()
        {

            if (this.ProconXml == null)
            {
                this.ProconXml = new XmlDocument();
            }

            XmlNodeList OptionsList = this.ProconXml.GetElementsByTagName("options");
            if (OptionsList.Count == 0)
            {
                this.ProconXml.AppendChild(this.ProconXml.CreateElement("options"));

                OptionsList = this.ProconXml.GetElementsByTagName("options");
            }

            XmlNodeList LicenseList = ((XmlElement)OptionsList[0]).GetElementsByTagName("licenses");
            if (LicenseList.Count == 0)
            {
                ((XmlElement)OptionsList[0]).AppendChild(this.ProconXml.CreateElement("licenses"));

                LicenseList = ((XmlElement)OptionsList[0]).GetElementsByTagName("licenses");
            }

            XmlNodeList previousAgreementList = ((XmlElement)LicenseList[0]).GetElementsByTagName("agreement");

            foreach (string agreementVersion in this.LicenseAgreements)
            {

                bool previouslyAdded = false;

                foreach (XmlNode previousAgreement in previousAgreementList)
                {
                    if (String.Compare(previousAgreement.InnerText, agreementVersion, true) == 0)
                    {
                        previouslyAdded = true;
                        break;
                    }
                }

                if (previouslyAdded == false)
                {
                    XmlNode agreement = this.ProconXml.CreateElement("agreement");
                    XmlAttribute agreementDate = this.ProconXml.CreateAttribute("stamp");
                    agreementDate.InnerText = DateTime.Now.ToShortDateString();
                    agreement.Attributes.Append(agreementDate);

                    agreement.InnerText = agreementVersion;

                    LicenseList[0].AppendChild(agreement);
                }
            }



            this.ProconXml.Save("PRoCon.xml");
        }


        public static bool IsProcessOpen()
        {

            int processCount = 0;

            try
            {
                Process currentProcess = Process.GetCurrentProcess();
                foreach (Process instance in Process.GetProcessesByName(currentProcess.ProcessName))
                {

                    if (String.Compare(instance.MainModule.FileName, currentProcess.MainModule.FileName) == 0)
                    {

                        processCount++;

                        if (processCount > 1)
                        {
                            break;
                        }
                    }

                }
            }
            // To catch permission exceptions
            catch
            {
                processCount = 0;
            }

            return (processCount > 1);
        }

        public PRoConApplication(bool consoleMode, string[] args)
        {

            this.LoadingMainConfig = true;
            this.LoadingAccountsFile = true;

            this.BlockUpdateChecks = false;
            this.MaxGspServers = int.MaxValue;
            this.CustomTitle = String.Empty;
            this.LicenseAgreements = new List<string>();

            int iValue;
            int iValue2;
            if (args != null && args.Length >= 2)
            {
                for (int i = 0; i < args.Length; i = i + 2)
                {
                    if (String.Compare("-name", args[i], true) == 0)
                    {
                        this.CustomTitle = args[i + 1];
                    }
                    else if (String.Compare("-maxservers", args[i], true) == 0 && int.TryParse(args[i + 1], out iValue) == true)
                    {
                        this.MaxGspServers = iValue;
                    }
                    else if (String.Compare("-blockupdatechecks", args[i], true) == 0 && int.TryParse(args[i + 1], out iValue) == true)
                    {
                        this.BlockUpdateChecks = (iValue == 1);
                    }
                    else if (String.Compare("-licensekey", args[i], true) == 0)
                    {
                        this.LicenseKey = args[i + 1];
                    }
                    else if (String.Compare("-plugin_max_runtime", args[i], true) == 0 && int.TryParse(args[i + 1], out iValue) == true && int.TryParse(args[i + 2], out iValue2) == true)
                    {
                        // transfered in this.LoadingMainConfig L-1333
                        this.praPluginMaxRuntime_m = iValue;
                        this.praPluginMaxRuntime_s = iValue2;
                        this.praPluginMaxRuntimeLocked = true;
                    }
                }
            }

            this.GetGspSettings();

            this.Connections = new ConnectionDictionary();
            this.Connections.ConnectionAdded += new ConnectionDictionary.ConnectionAlteredHandler(Connections_ConnectionAdded);
            this.Connections.ConnectionRemoved += new ConnectionDictionary.ConnectionAlteredHandler(Connections_ConnectionRemoved);

            this.OptionsSettings = new OptionsSettings(this);
            this.OptionsSettings.ChatLoggingChanged += new OptionsSettings.OptionsEnabledHandler(OptionsSettings_ChatLoggingChanged);
            this.OptionsSettings.ConsoleLoggingChanged += new OptionsSettings.OptionsEnabledHandler(OptionsSettings_ConsoleLoggingChanged);
            this.OptionsSettings.EventsLoggingChanged += new OptionsSettings.OptionsEnabledHandler(OptionsSettings_EventsLoggingChanged);
            this.OptionsSettings.PluginsLoggingChanged += new Options.OptionsSettings.OptionsEnabledHandler(OptionsSettings_PluginsLoggingChanged);

            this.Languages = new LocalizationDictionary();
            if ((this.ConsoleMode = consoleMode) == false)
            {
                this.LoadLocalizationFiles();
            }

            if (this.Languages.Contains("au.loc") == true)
            {
                this.CurrentLanguage = this.Languages["au.loc"];
            }
            else
            {
                this.CurrentLanguage = new CLocalization();
            }

            this.AutoUpdater = new AutoUpdater(this, args);

            this.AccountsList = new AccountDictionary();
            this.AccountsList.AccountAdded += new AccountDictionary.AccountAlteredHandler(AccountsList_AccountAdded);
            this.AccountsList.AccountRemoved += new AccountDictionary.AccountAlteredHandler(AccountsList_AccountRemoved);
            // TODO: Password change -> Save

            this.m_clIpToCountry = new CountryLookup(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GeoIP.dat"));

            this.SavedWindowBounds = new Rectangle();

            this.RegexMatchPunkbusterPlist = new Regex(@":[ ]+?(?<slotid>[0-9]+)[ ]+?(?<guid>[A-Fa-f0-9]+)\(.*?\)[ ]+?(?<ip>[0-9\.:]+).*?\(.*?\)[ ]+?""(?<name>.*?)\""", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            this.RegexMatchPunkbusterGuidComputed = new Regex(@":[ ]+?Player Guid Computed[ ]+?(?<guid>[A-Fa-f0-9]+)\(.*?\)[ ]+?\(slot #(?<slotid>[0-9]+)\)[ ]+?(?<ip>[0-9\.:]+)[ ]+?(?<name>.*)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            this.RegexMatchPunkbusterBanlist = new Regex(@":[ ]+?(?<banid>[0-9]+)[ ]+?(?<guid>[A-Fa-f0-9]+)[ ]+?{(?<remaining>[0-9\-]+)/(?<banlength>[0-9\-]+)}[ ]+?""(?<name>.+?)""[ ]+?""(?<ip>.+?)""[ ]+?(?<reason>.*)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            this.RegexMatchPunkbusterUnban = new Regex(@":[ ]+?Guid[ ]+?(?<guid>[A-Fa-f0-9]+)[ ]+?has been Unbanned", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            this.RegexMatchPunkbusterBanAdded = new Regex(@": Ban Added to Ban List", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            // PunkBuster Server: Kick/Ban Command Issued (testing) for (slot#1) aaa.bbb.ccc.ddd:3659 guidstring namestring
            //this.RegexMatchPunkbusterKickBanCmd = new Regex(@": Kick\/Ban Command Issued \((?<reason>.*)\) for \(slot#(?<slotid>[0-9]+)\) (?<ip>[0-9\.:]+) (?<guid>[A-Fa-f0-9]+) (?<name>.*)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            //this.RegexMatchPunkbusterKickBanCmd = new Regex(@":[ ]+?(Kick|Ban)[ ]+?Command[ ]+?Issued[ ]+?\((?<reason>.*)\)[ ]+?for[ ]+?\(slot\#(?<slotid>[0-9]+)\)[ ]+?(?<ip>[0-9\.:]+)[ ]+(?<guid>[A-Fa-f0-9]+)[ ]+?(?<name>.*)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            this.RegexMatchPunkbusterKickBanCmd = new Regex(@":[ ]+?(?<kb_type>Kick(\/Ban)?)[ ]+?Command[ ]+?Issued[ ]+?\((?<reason>.*)\)[ ]+?for[ ]+?\(slot\#(?<slotid>[0-9]+)\)[ ]+?(?<ip>[0-9\.:]+)[ ]+(?<guid>[A-Fa-f0-9]+)[ ]+?(?<name>.*)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

            this.RegexMatchPunkbusterBeginPlist = new Regex(@":[ ]+?Player List: ", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            this.RegexMatchPunkbusterEndPlist = new Regex(@":[ ]+?End of Player List \((?<players>[0-9]+) Players\)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

            //this.CleanPlugins();

            // Create the initial web server object
            this.ExecutePRoConCommand(this, new List<string>() { "procon.private.httpWebServer.enable", "false", "27360", "0.0.0.0" }, 0);

            //this.Execute();
        }

        public void Execute()
        {
            // Load all of the accounts.
            this.UpdateRss();

            this.ExecuteMainConfig("accounts.cfg");
            this.LoadingAccountsFile = false;

            if (this.praPluginMaxRuntimeLocked == true)
            {
                this.OptionsSettings.PluginMaxRuntimeLocked = this.praPluginMaxRuntimeLocked;
                this.OptionsSettings.PluginMaxRuntime_m = this.praPluginMaxRuntime_m;
                this.OptionsSettings.PluginMaxRuntime_s = this.m_praPluginMaxRuntime_s;
            }

            this.ExecuteMainConfig("procon.cfg");
            this.LoadingMainConfig = false;

            this.Checker = new Timer(o => this.ReconnectVersionChecker(), null, 20000, 20000);
        }

        private void HttpWebServer_ProcessRequest(HttpWebServerRequest sender)
        {

            string[] directories = sender.Data.RequestPath.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
            HttpWebServerResponseData response = new HttpWebServerResponseData(String.Empty);

            if (directories.Length == 0)
            {

                switch (sender.Data.RequestFile.ToLower())
                {
                    case "connections":
                        response.Document = this.Connections.ToJsonString();
                        response.Cache.CacheType = PRoCon.Core.HttpServer.Cache.HttpWebServerCacheType.NoCache;
                        break;
                    default:
                        response.StatusCode = "404 Not Found";
                        break;
                }
            }
            else if (directories.Length == 1)
            {

                if (this.Connections.Contains(directories[0]) == true)
                {

                    switch (sender.Data.RequestFile.ToLower())
                    {
                        case "players":

                            response.Document = this.Connections[directories[0]].PlayerList.ToJsonString();
                            response.Cache.CacheType = PRoCon.Core.HttpServer.Cache.HttpWebServerCacheType.NoCache;
                            break;
                        case "chat":

                            int historyLength = 0;
                            DateTime newerThan = DateTime.Now;

                            if (int.TryParse(sender.Data.Query.Get("history"), out historyLength) == true)
                            {
                                response.Document = this.Connections[directories[0]].ChatConsole.ToJsonString(historyLength);
                                response.Cache.CacheType = PRoCon.Core.HttpServer.Cache.HttpWebServerCacheType.NoCache;
                            }
                            else if (DateTime.TryParse(sender.Data.Query.Get("newer_than"), out newerThan) == true)
                            {
                                response.Document = this.Connections[directories[0]].ChatConsole.ToJsonString(newerThan.ToLocalTime());
                            }
                            else
                            {
                                response.StatusCode = "400 Bad Request";
                            }

                            break;
                        default:

                            response.StatusCode = "404 Not Found";
                            break;
                    }
                }

            }
            else if (directories.Length >= 3)
            {

                // /HostNameIp/plugins/PluginClassName/
                if (this.Connections.Contains(directories[0]) == true && String.Compare("plugins", directories[1]) == 0)
                {

                    if (this.Connections[directories[0]].PluginsManager.Plugins.EnabledClassNames.Contains(directories[2]) == true)
                    {
                        HttpWebServerResponseData pluginRespose = (HttpWebServerResponseData)this.Connections[directories[0]].PluginsManager.InvokeOnEnabled(directories[2], "OnHttpRequest", sender.Data);

                        if (pluginRespose != null)
                        {
                            response = pluginRespose;
                        }
                    }
                    else
                    {
                        response.StatusCode = "404 Not Found";
                    }
                }
                else
                {
                    response.StatusCode = "404 Not Found";
                }
            }
            else
            {
                response.StatusCode = "404 Not Found";
            }

            sender.Respond(response);
        }

        private void Connections_ConnectionAdded(PRoConClient item)
        {
            this.SaveMainConfig();
            item.AutomaticallyConnectChanged += new PRoConClient.AutomaticallyConnectHandler(item_AutomaticallyConnectChanged);
        }

        private void Connections_ConnectionRemoved(PRoConClient item)
        {
            item.AutomaticallyConnectChanged -= new PRoConClient.AutomaticallyConnectHandler(item_AutomaticallyConnectChanged);
            this.SaveMainConfig();
            item.ForceDisconnect();
            item.Destroy();
        }

        private void item_AutomaticallyConnectChanged(PRoConClient sender, bool isEnabled)
        {
            this.SaveMainConfig();
        }

        void OptionsSettings_PluginsLoggingChanged(bool blEnabled)
        {
            foreach (PRoConClient prcClient in this.Connections)
            {
                if (prcClient.PluginConsole != null)
                {
                    prcClient.PluginConsole.Logging = blEnabled;
                }
            }
        }

        void OptionsSettings_EventsLoggingChanged(bool blEnabled)
        {
            foreach (PRoConClient prcClient in this.Connections)
            {
                if (prcClient.EventsLogging != null)
                {
                    prcClient.EventsLogging.Logging = blEnabled;
                }
            }
        }

        void OptionsSettings_ConsoleLoggingChanged(bool blEnabled)
        {
            foreach (PRoConClient prcClient in this.Connections)
            {
                if (prcClient.Console != null)
                {
                    prcClient.Console.Logging = blEnabled;
                }
            }
        }

        private void OptionsSettings_ChatLoggingChanged(bool blEnabled)
        {
            foreach (PRoConClient prcClient in this.Connections)
            {
                if (prcClient.ChatConsole != null)
                {
                    prcClient.ChatConsole.Logging = blEnabled;
                }
            }
        }

        public PRoConClient AddConnection(string strHost, UInt16 iu16Port, string strUsername, string strPassword)
        {
            PRoConClient prcNewClient = null;

            if (this.Connections.Contains(strHost + ":" + iu16Port.ToString()) == false && this.Connections.Count < this.MaxGspServers)
            {
                prcNewClient = new PRoConClient(this, strHost, iu16Port, strUsername, strPassword);

                this.Connections.Add(prcNewClient);

                this.SaveMainConfig();
            }

            return prcNewClient;
        }

        #region Loading/Saving Configs and Commands


        public void LoadLocalizationFiles()
        {

            lock (new object())
            {

                string strCurrentLanguagePath = String.Empty;

                if (this.CurrentLanguage != null)
                {
                    strCurrentLanguagePath = this.CurrentLanguage.FilePath;
                }

                this.Languages.Clear();

                try
                {

                    DirectoryInfo diLocalizationDir = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Localization"));
                    FileInfo[] a_fiLocalizations = diLocalizationDir.GetFiles("*.loc");

                    foreach (FileInfo fiLocalization in a_fiLocalizations)
                    {

                        CLocalization clocLoadedLanguage = this.Languages.LoadLocalizationFile(fiLocalization.FullName, fiLocalization.Name);

                        //CLocalization clocLoadedLanguage = new CLocalization(fiLocalization.Name);

                        //if (this.Languages.Contains(clocLoadedLanguage.FileName) == false) {
                        //    this.Languages.Add(clocLoadedLanguage);

                        if (String.Compare(clocLoadedLanguage.FilePath, strCurrentLanguagePath, true) == 0)
                        {
                            this.CurrentLanguage = clocLoadedLanguage;
                        }
                        //}
                    }
                }
                catch (Exception e)
                {
                    FrostbiteConnection.LogError(String.Empty, String.Empty, e);
                }
            }
        }

        private void ExecuteMainConfig(string strConfigFile)
        {

            if (File.Exists(Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs"), strConfigFile)) == true)
            {

                string[] a_strLines = File.ReadAllLines(Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs"), strConfigFile), Encoding.UTF8);

                foreach (string strLine in a_strLines)
                {

                    List<string> lstWords = Packet.Wordify(strLine);

                    if (lstWords.Count >= 1 && Regex.Match(strLine, "^[ ]*//.*").Success == false)
                    {
                        this.ExecutePRoConCommand(this, lstWords, 0);
                    }
                }
            }
        }

        public void SaveMainConfig()
        {

            if (this.LoadingMainConfig == false && this.CurrentLanguage != null && this.OptionsSettings != null && this.Connections != null && this.HttpWebServer != null)
            {
                FileStream stmProconConfigFile = null;

                try
                {

                    if (Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs")) == false)
                    {
                        Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs"));
                    }

                    stmProconConfigFile = new FileStream(Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs"), "procon.cfg"), FileMode.Create);

                    if (stmProconConfigFile != null)
                    {
                        StreamWriter stwConfig = new StreamWriter(stmProconConfigFile, Encoding.UTF8);

                        stwConfig.WriteLine("/////////////////////////////////////////////");
                        stwConfig.WriteLine("// This config will be overwritten by procon.");
                        stwConfig.WriteLine("/////////////////////////////////////////////");

                        //foreach (string[] a_strUsernamePassword in this.m_frmManageAccounts.UserList) {
                        //    stwConfig.WriteLine("procon.public.accounts.create \"{0}\" \"{1}\"", a_strUsernamePassword[0], a_strUsernamePassword[1]);
                        //}

                        stwConfig.WriteLine("procon.private.window.position {0} {1} {2} {3} {4}", this.SavedWindowState, this.SavedWindowBounds.X, this.SavedWindowBounds.Y, this.SavedWindowBounds.Width, this.SavedWindowBounds.Height);
                        //stwConfig.WriteLine("procon.private.window.splitterPosition {0}", this.spltTreeServers.SplitterDistance);

                        stwConfig.WriteLine("procon.private.options.setLanguage \"{0}\"", this.CurrentLanguage.FileName);
                        stwConfig.WriteLine("procon.private.options.chatLogging {0}", this.OptionsSettings.ChatLogging);
                        stwConfig.WriteLine("procon.private.options.consoleLogging {0}", this.OptionsSettings.ConsoleLogging);
                        stwConfig.WriteLine("procon.private.options.eventsLogging {0}", this.OptionsSettings.EventsLogging);
                        stwConfig.WriteLine("procon.private.options.pluginLogging {0}", this.OptionsSettings.PluginLogging);
                        stwConfig.WriteLine("procon.private.options.autoCheckDownloadUpdates {0}", this.OptionsSettings.AutoCheckDownloadUpdates);
                        stwConfig.WriteLine("procon.private.options.autoApplyUpdates {0}", this.OptionsSettings.AutoApplyUpdates);
                        stwConfig.WriteLine("procon.private.options.autoCheckGameConfigsForUpdates {0}", this.OptionsSettings.AutoCheckGameConfigsForUpdates);
                        stwConfig.WriteLine("procon.private.options.showtrayicon {0}", this.OptionsSettings.ShowTrayIcon);
                        stwConfig.WriteLine("procon.private.options.minimizetotray {0}", this.OptionsSettings.MinimizeToTray);
                        stwConfig.WriteLine("procon.private.options.closetotray {0}", this.OptionsSettings.CloseToTray);

                        stwConfig.WriteLine("procon.private.options.allowanonymoususagedata {0}", this.OptionsSettings.AllowAnonymousUsageData);

                        stwConfig.WriteLine("procon.private.options.runPluginsInSandbox {0}", this.OptionsSettings.RunPluginsInTrustedSandbox);
                        stwConfig.WriteLine("procon.private.options.allowAllODBCConnections {0}", this.OptionsSettings.AllowAllODBCConnections);
                        stwConfig.WriteLine("procon.private.options.allowAllSmtpConnections {0}", this.OptionsSettings.AllowAllSmtpConnections);

                        stwConfig.WriteLine("procon.private.options.adminMoveMessage {0}", this.OptionsSettings.AdminMoveMessage);
                        stwConfig.WriteLine("procon.private.options.chatDisplayAdminName {0}", this.OptionsSettings.ChatDisplayAdminName);
                        stwConfig.WriteLine("procon.private.options.EnableAdminReason {0}", this.OptionsSettings.EnableAdminReason);

                        stwConfig.WriteLine("procon.private.options.layerHideLocalPlugins {0}", this.OptionsSettings.LayerHideLocalPlugins);
                        stwConfig.WriteLine("procon.private.options.layerHideLocalAccounts {0}", this.OptionsSettings.LayerHideLocalAccounts);

                        stwConfig.WriteLine("procon.private.options.ShowRoundTimerConstantly {0}", this.OptionsSettings.ShowRoundTimerConstantly);
                        stwConfig.WriteLine("procon.private.options.ShowCfmMsgRoundRestartNext {0}", this.OptionsSettings.ShowCfmMsgRoundRestartNext);

                        stwConfig.WriteLine("procon.private.options.ShowDICESpecialOptions {0}", this.OptionsSettings.ShowDICESpecialOptions);

                        if (this.HttpWebServer != null)
                        {
                            stwConfig.WriteLine("procon.private.httpWebServer.enable {0} {1} \"{2}\"", this.HttpWebServer.IsOnline, this.HttpWebServer.ListeningPort, this.HttpWebServer.BindingAddress);
                        }

                        stwConfig.Write("procon.private.options.trustedHostDomainsPorts");
                        foreach (TrustedHostWebsitePort trusted in this.OptionsSettings.TrustedHostsWebsitesPorts)
                        {
                            stwConfig.Write(" {0} {1}", trusted.HostWebsite, trusted.Port);
                        }
                        stwConfig.WriteLine(String.Empty);
                        //stwConfig.WriteLine("procon.private.options.trustedHostDomainsPorts {0}", String.Join(" ", this.m_frmOptions.TrustedHostDomainsPorts.ToArray()));

                        if (this.OptionsSettings.StatsLinksMaxNum > 4)
                        {
                            stwConfig.Write("procon.private.options.statsLinksMaxNum");
                            stwConfig.Write(" {0}", this.OptionsSettings.StatsLinksMaxNum.ToString());
                            stwConfig.WriteLine(String.Empty);
                        }

                        if (this.OptionsSettings.StatsLinkNameUrl.Count > 0)
                        {
                            stwConfig.Write("procon.private.options.statsLinkNameUrl");
                            foreach (StatsLinkNameUrl statsLink in this.OptionsSettings.StatsLinkNameUrl)
                            {
                                stwConfig.Write(" {0} {1}", statsLink.LinkName, statsLink.LinkUrl);
                            }
                            stwConfig.WriteLine(String.Empty);
                        }

                        if ((this.OptionsSettings.PluginMaxRuntime_m > 0 || this.OptionsSettings.PluginMaxRuntime_s > 0) && this.OptionsSettings.PluginMaxRuntimeLocked == false)
                        {
                            stwConfig.WriteLine("procon.private.options.pluginMaxRuntime {0} {1}", this.OptionsSettings.PluginMaxRuntime_m, this.OptionsSettings.PluginMaxRuntime_s);
                        }

                        stwConfig.WriteLine("procon.private.options.UsePluginOldStyleLoad {0}", this.OptionsSettings.UsePluginOldStyleLoad);

                        stwConfig.WriteLine("procon.private.options.enablePluginDebugging {0}", this.OptionsSettings.EnablePluginDebugging);

                        stwConfig.WriteLine("procon.private.options.UseGeoIpFileOnly {0}", this.OptionsSettings.UseGeoIpFileOnly);
                        stwConfig.WriteLine("procon.private.options.BlockRssFeedNews {0}", this.OptionsSettings.BlockRssFeedNews);

                        foreach (PRoConClient prcClient in this.Connections)
                        {

                            string strAddServerCommand = String.Format("procon.private.servers.add \"{0}\" {1}", prcClient.HostName, prcClient.Port);

                            if (prcClient.Password.Length > 0)
                            {
                                strAddServerCommand = String.Format("{0} \"{1}\"", strAddServerCommand, prcClient.Password);

                                if (prcClient.Username.Length > 0)
                                {
                                    strAddServerCommand = String.Format("{0} \"{1}\"", strAddServerCommand, prcClient.Username);
                                }
                            }

                            // new position before label
                            stwConfig.WriteLine(strAddServerCommand);

                            if (prcClient.CurrentServerInfo != null || prcClient.ConnectionServerName != String.Empty)
                            {
                                if (prcClient.CurrentServerInfo != null)
                                {
                                    stwConfig.WriteLine("procon.private.servers.name \"{0}\" {1} \"{2}\"", prcClient.HostName, prcClient.Port, prcClient.CurrentServerInfo.ServerName);
                                }
                                else
                                {
                                    stwConfig.WriteLine("procon.private.servers.name \"{0}\" {1} \"{2}\"", prcClient.HostName, prcClient.Port, prcClient.ConnectionServerName);
                                }
                            }

                            // stwConfig.WriteLine(strAddServerCommand);

                            if (prcClient.AutomaticallyConnect == true)
                            {
                                stwConfig.WriteLine("procon.private.servers.autoconnect \"{0}\" {1}", prcClient.HostName, prcClient.Port);
                            }
                        }

                        stwConfig.Close();
                    }
                }
                catch (Exception e)
                {
                    FrostbiteConnection.LogError("SaveMainConfig", String.Empty, e);
                }
                finally
                {
                    if (stmProconConfigFile != null)
                    {
                        stmProconConfigFile.Close();
                    }
                }
            }
        }

        public void ExecutePRoConCommand(object objSender, List<string> lstWords, int iRecursion)
        {

            if (lstWords.Count >= 4 && String.Compare(lstWords[0], "procon.protected.weapons.add", true) == 0 && objSender is PRoConClient)
            {

                if (((PRoConClient)objSender).Weapons.Contains(lstWords[2]) == false &&
                    Enum.IsDefined(typeof(Kits), lstWords[1]) == true &&
                    Enum.IsDefined(typeof(WeaponSlots), lstWords[3]) == true &&
                    Enum.IsDefined(typeof(DamageTypes), lstWords[4]) == true)
                {
                    //this.SavedWindowState = (System.Windows.Forms.FormWindowState)Enum.Parse(typeof(System.Windows.Forms.FormWindowState), lstWords[1]);

                    ((PRoConClient)objSender).Weapons.Add(
                            new Weapon(
                                (Kits)Enum.Parse(typeof(Kits), lstWords[1]),
                                lstWords[2],
                                (WeaponSlots)Enum.Parse(typeof(WeaponSlots), lstWords[3]),
                                (DamageTypes)Enum.Parse(typeof(DamageTypes), lstWords[4])
                            )
                        );
                }
            }
            else if (lstWords.Count >= 1 && String.Compare(lstWords[0], "procon.protected.weapons.clear", true) == 0 && objSender is PRoConClient)
            {
                ((PRoConClient)objSender).Weapons.Clear();
            }
            else if (lstWords.Count >= 1 && String.Compare(lstWords[0], "procon.protected.zones.clear", true) == 0 && objSender is PRoConClient)
            {
                ((PRoConClient)objSender).MapGeometry.MapZones.Clear();
            }
            else if (lstWords.Count >= 4 && String.Compare(lstWords[0], "procon.protected.zones.add", true) == 0 && objSender is PRoConClient)
            {

                List<Point3D> points = new List<Point3D>();
                int iPoints = 0;

                if (int.TryParse(lstWords[4], out iPoints) == true)
                {

                    for (int i = 0, iOffset = 5; i < iPoints && iOffset + 3 <= lstWords.Count; i++)
                    {
                        points.Add(new Point3D(lstWords[iOffset++], lstWords[iOffset++], lstWords[iOffset++]));
                    }
                }

                if (((PRoConClient)objSender).MapGeometry.MapZones.Contains(lstWords[1]) == false)
                {
                    ((PRoConClient)objSender).MapGeometry.MapZones.Add(new MapZoneDrawing(lstWords[1], lstWords[2], lstWords[3], points.ToArray(), true));
                }
            }


            else if (lstWords.Count >= 3 && String.Compare(lstWords[0], "procon.protected.specialization.add", true) == 0 && objSender is PRoConClient)
            {

                if (((PRoConClient)objSender).Specializations.Contains(lstWords[2]) == false &&
                    Enum.IsDefined(typeof(SpecializationSlots), lstWords[1]) == true)
                {

                    ((PRoConClient)objSender).Specializations.Add(new Specialization((SpecializationSlots)Enum.Parse(typeof(SpecializationSlots), lstWords[1]), lstWords[2]));
                }

            }
            else if (lstWords.Count >= 1 && String.Compare(lstWords[0], "procon.protected.specialization.clear", true) == 0 && objSender is PRoConClient)
            {
                ((PRoConClient)objSender).Specializations.Clear();
            }
            else if (lstWords.Count >= 5 && String.Compare(lstWords[0], "procon.protected.teamnames.add", true) == 0 && objSender is PRoConClient)
            {

                if (lstWords.Count >= 6)
                {
                    int iTeamID = 0;
                    if (int.TryParse(lstWords[2], out iTeamID) == true)
                    {
                        ((PRoConClient)objSender).ProconProtectedTeamNamesAdd(lstWords[1], iTeamID, lstWords[3], lstWords[4], lstWords[5]);
                    }
                }
                else
                {
                    int iTeamID = 0;
                    if (int.TryParse(lstWords[2], out iTeamID) == true)
                    {
                        ((PRoConClient)objSender).ProconProtectedTeamNamesAdd(lstWords[1], iTeamID, lstWords[3], lstWords[4]);
                    }
                }
            }
            else if (lstWords.Count >= 6 && String.Compare(lstWords[0], "procon.protected.maps.add", true) == 0 && objSender is PRoConClient)
            {
                int iDefaultSquadID = 0;
                if (int.TryParse(lstWords[5], out iDefaultSquadID) == true)
                {
                    ((PRoConClient)objSender).ProconProtectedMapsAdd(lstWords[1], lstWords[2], lstWords[3], lstWords[4], iDefaultSquadID);
                }
            }
            else if (lstWords.Count >= 4 && String.Compare(lstWords[0], "procon.protected.plugins.setVariable", true) == 0 && objSender is PRoConClient)
            {

                string strUnescapedNewlines = lstWords[3].Replace(@"\n", "\n");
                strUnescapedNewlines = strUnescapedNewlines.Replace(@"\r", "\r");
                strUnescapedNewlines = strUnescapedNewlines.Replace(@"\""", "\"");

                ((PRoConClient)objSender).ProconProtectedPluginSetVariable(lstWords[1], lstWords[2], strUnescapedNewlines);
            }
            else if (lstWords.Count >= 3 && String.Compare(lstWords[0], "procon.protected.vars.set", true) == 0 && objSender is PRoConClient)
            {
                ((PRoConClient)objSender).Variables.SetVariable(lstWords[1], lstWords[2]);
            }
            else if (lstWords.Count >= 3 && String.Compare(lstWords[0], "procon.protected.layer.setPrivileges", true) == 0 && objSender is PRoConClient)
            {

                CPrivileges sprPrivs = new CPrivileges();
                UInt32 ui32Privileges = 0;

                if (UInt32.TryParse(lstWords[2], out ui32Privileges) == true)
                {
                    sprPrivs.PrivilegesFlags = ui32Privileges;
                    if (this.AccountsList.Contains(lstWords[1]) == true)
                    {
                        ((PRoConClient)objSender).ProconProtectedLayerSetPrivileges(this.AccountsList[lstWords[1]], sprPrivs);
                    }
                }
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.protected.send", true) == 0 && objSender is PRoConClient)
            {
                lstWords.RemoveAt(0);

                // Block them from changing the admin password to X
                if (String.Compare(lstWords[0], "vars.adminPassword", true) != 0)
                {
                    ((PRoConClient)objSender).SendRequest(lstWords);
                }
            }
            else if (lstWords.Count >= 3 && String.Compare(lstWords[0], "procon.public.accounts.create", true) == 0)
            {
                this.AccountsList.CreateAccount(lstWords[1], lstWords[2]);
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.public.accounts.delete", true) == 0)
            {
                this.AccountsList.DeleteAccount(lstWords[1]);
            }
            else if (lstWords.Count >= 3 && String.Compare(lstWords[0], "procon.public.accounts.setPassword", true) == 0)
            {
                this.AccountsList.ChangePassword(lstWords[1], lstWords[2]);
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.protected.config.exec", true) == 0 && objSender is PRoConClient)
            {
                if (iRecursion < 5)
                {
                    ((PRoConClient)objSender).ExecuteConnectionConfig(lstWords[1], iRecursion, lstWords.Count > 2 ? lstWords.GetRange(2, lstWords.Count - 2) : null, false);
                }
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.protected.pluginconsole.write", true) == 0 && objSender is PRoConClient)
            {
                ((PRoConClient)objSender).PluginConsole.Write(lstWords[1]);
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.protected.console.write", true) == 0 && objSender is PRoConClient)
            {
                ((PRoConClient)objSender).Console.Write(lstWords[1]);
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.protected.chat.write", true) == 0 && objSender is PRoConClient)
            {
                ((PRoConClient)objSender).ChatConsole.WriteViaCommand(lstWords[1]);
            }
            else if (lstWords.Count >= 5 && String.Compare(lstWords[0], "procon.protected.events.write", true) == 0 && objSender is PRoConClient)
            {

                // EventType etType, CapturableEvents ceEvent, string strEventText, DateTime dtLoggedTime, string instigatingAdmin

                if (Enum.IsDefined(typeof(EventType), lstWords[1]) == true && Enum.IsDefined(typeof(CapturableEvents), lstWords[2]) == true)
                {

                    EventType type = (EventType)Enum.Parse(typeof(EventType), lstWords[1]);
                    CapturableEvents cappedEventType = (CapturableEvents)Enum.Parse(typeof(CapturableEvents), lstWords[2]);

                    CapturedEvent cappedEvent = new CapturedEvent(type, cappedEventType, lstWords[3], DateTime.Now, lstWords[4]);

                    ((PRoConClient)objSender).EventsLogging.ProcessEvent(cappedEvent);
                }
            }

            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.protected.layer.enable", true) == 0 && objSender is PRoConClient)
            {
                // procon.protected.layer.enable <true> <port>
                bool blEnabled = false;
                UInt16 ui16Port = 0;

                if (bool.TryParse(lstWords[1], out blEnabled) == true)
                {

                    if (lstWords.Count >= 5)
                    {
                        UInt16.TryParse(lstWords[2], out ui16Port);
                        ((PRoConClient)objSender).ProconProtectedLayerEnable(blEnabled, ui16Port, lstWords[3], lstWords[4]);
                    }
                    else
                    {
                        ((PRoConClient)objSender).ProconProtectedLayerEnable(blEnabled, 27260, "0.0.0.0", "PRoCon[%servername%]");
                    }
                }
            }
            else if (lstWords.Count >= 1 && String.Compare(lstWords[0], "procon.protected.teamnames.clear", true) == 0 && objSender is PRoConClient)
            {
                ((PRoConClient)objSender).ProconProtectedTeamNamesClear();
            }
            else if (lstWords.Count >= 1 && String.Compare(lstWords[0], "procon.protected.maps.clear", true) == 0 && objSender is PRoConClient)
            {
                ((PRoConClient)objSender).ProconProtectedMapsClear();
            }
            else if (lstWords.Count >= 1 && String.Compare(lstWords[0], "procon.protected.reasons.clear", true) == 0 && objSender is PRoConClient)
            {
                ((PRoConClient)objSender).ProconProtectedReasonsClear();
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.protected.reasons.add", true) == 0 && objSender is PRoConClient)
            {
                ((PRoConClient)objSender).ProconProtectedReasonsAdd(lstWords[1]);
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.protected.serverversions.clear", true) == 0 && objSender is PRoConClient)
            {
                ((PRoConClient)objSender).ProconProtectedServerVersionsClear(lstWords[1]);
            }
            else if (lstWords.Count >= 4 && String.Compare(lstWords[0], "procon.protected.serverversions.add", true) == 0 && objSender is PRoConClient)
            {
                ((PRoConClient)objSender).ProconProtectedServerVersionsAdd(lstWords[1], lstWords[2], lstWords[3]);
            }
            else if (lstWords.Count >= 3 && String.Compare(lstWords[0], "procon.protected.plugins.enable", true) == 0 && objSender is PRoConClient)
            {

                bool blEnabled = false;

                if (bool.TryParse(lstWords[2], out blEnabled) == true)
                {
                    ((PRoConClient)objSender).ProconProtectedPluginEnable(lstWords[1], blEnabled);
                }
            }
            else if (lstWords.Count >= 3 && String.Compare(lstWords[0], "procon.private.servers.add", true) == 0 && objSender == this)
            {
                // add IP port [password [username]]
                // procon.private.servers.add "127.0.0.1" 27260 "Password" "Phogue"
                UInt16 ui16Port = 0;
                if (UInt16.TryParse(lstWords[2], out ui16Port) == true)
                {
                    if (lstWords.Count == 3)
                    {
                        this.AddConnection(lstWords[1], ui16Port, String.Empty, String.Empty);
                    }
                    else if (lstWords.Count == 4)
                    {
                        this.AddConnection(lstWords[1], ui16Port, String.Empty, lstWords[3]);
                    }
                    else if (lstWords.Count == 5)
                    {
                        this.AddConnection(lstWords[1], ui16Port, lstWords[4], lstWords[3]);
                    }
                }
            }
            else if (lstWords.Count >= 3 && String.Compare(lstWords[0], "procon.private.servers.connect", true) == 0 && objSender == this)
            {

                if (this.Connections.Contains(lstWords[1] + ":" + lstWords[2]) == true)
                {
                    this.Connections[lstWords[1] + ":" + lstWords[2]].ProconPrivateServerConnect();
                }
            }
            else if (lstWords.Count >= 3 && String.Compare(lstWords[0], "procon.private.servers.autoconnect", true) == 0 && objSender == this)
            {
                if (this.Connections.Contains(lstWords[1] + ":" + lstWords[2]) == true)
                {

                    this.Connections[lstWords[1] + ":" + lstWords[2]].AutomaticallyConnect = true;

                    // Originally leaving it for the reconnect thread to pickup but needed a quicker effect.
                    this.Connections[lstWords[1] + ":" + lstWords[2]].ProconPrivateServerConnect();
                }
            }

            else if (lstWords.Count >= 3 && String.Compare(lstWords[0], "procon.private.servers.name", true) == 0 && objSender == this)
            {
                // CurrentServerInfo not initialized.
                if (this.Connections.Contains(lstWords[1] + ":" + lstWords[2]) == true)
                {
                    this.Connections[lstWords[1] + ":" + lstWords[2]].ConnectionServerName = lstWords[3];
                }
            }

            /*
            else if (lstWords.Count >= 3 && String.Compare(lstWords[0], "procon.private.servers.name", true) == 0 && objSender == this) {
                this.uscServerPlayerTreeviewListing.SetServerName(lstWords[1] + ":" + lstWords[2], lstWords[3]);
            }

            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.private.window.splitterPosition", true) == 0 && objSender == this) {
                int iPositionVar = 0;

                if (int.TryParse(lstWords[1], out iPositionVar) == true) {
                    if (iPositionVar >= this.spltTreeServers.Panel1MinSize && iPositionVar <= this.spltTreeServers.Width - this.spltTreeServers.Panel2MinSize) {
                        this.spltTreeServers.SplitterDistance = iPositionVar;
                    }
                }
            }
            */

            else if (lstWords.Count >= 6 && String.Compare(lstWords[0], "procon.private.window.position", true) == 0 && objSender == this)
            {

                Rectangle recWindowBounds = new Rectangle(0, 0, 1024, 768);
                int iPositionVar = 0;

                if (Enum.IsDefined(typeof(System.Windows.Forms.FormWindowState), lstWords[1]) == true)
                {
                    this.SavedWindowState = (System.Windows.Forms.FormWindowState)Enum.Parse(typeof(System.Windows.Forms.FormWindowState), lstWords[1]);

                    if (int.TryParse(lstWords[2], out iPositionVar) == true)
                    {
                        if (iPositionVar >= 0)
                        {
                            recWindowBounds.X = iPositionVar;
                        }
                    }

                    if (int.TryParse(lstWords[3], out iPositionVar) == true)
                    {
                        if (iPositionVar >= 0)
                        {
                            recWindowBounds.Y = iPositionVar;
                        }
                    }

                    if (int.TryParse(lstWords[4], out iPositionVar) == true)
                    {
                        recWindowBounds.Width = iPositionVar;
                    }

                    if (int.TryParse(lstWords[5], out iPositionVar) == true)
                    {
                        recWindowBounds.Height = iPositionVar;
                    }

                    this.SavedWindowBounds = recWindowBounds;
                }

            }







            // procon.private.httpWebServer.enable true 27360 "0.0.0.0"
            else if (lstWords.Count >= 4 && String.Compare(lstWords[0], "procon.private.httpWebServer.enable", true) == 0 && objSender == this)
            {

                bool blEnabled = false;
                string bindingAddress = "0.0.0.0";
                UInt16 ui16Port = 27360;

                if (bool.TryParse(lstWords[1], out blEnabled) == true)
                {

                    if (this.HttpWebServer != null)
                    {
                        this.HttpWebServer.Shutdown();

                        this.HttpWebServer.ProcessRequest -= new HttpWebServer.ProcessResponseHandler(HttpWebServer_ProcessRequest);
                        this.HttpWebServer.HttpServerOnline -= new HttpWebServer.StateChangeHandler(HttpWebServer_HttpServerOnline);
                        this.HttpWebServer.HttpServerOffline -= new HttpWebServer.StateChangeHandler(HttpWebServer_HttpServerOffline);
                    }

                    bindingAddress = lstWords[3];
                    if (UInt16.TryParse(lstWords[2], out ui16Port) == false)
                    {
                        ui16Port = 27360;
                    }

                    this.HttpWebServer = new HttpWebServer(bindingAddress, ui16Port);
                    this.HttpWebServer.ProcessRequest += new HttpWebServer.ProcessResponseHandler(HttpWebServer_ProcessRequest);
                    this.HttpWebServer.HttpServerOnline += new HttpWebServer.StateChangeHandler(HttpWebServer_HttpServerOnline);
                    this.HttpWebServer.HttpServerOffline += new HttpWebServer.StateChangeHandler(HttpWebServer_HttpServerOffline);

                    if (blEnabled == true)
                    {
                        this.HttpWebServer.Start();
                    }
                }
            }

            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.private.options.setLanguage", true) == 0 && objSender == this)
            {

                // if it does not exist but they have explicity asked for it, see if we can load it up
                // this could not be loaded because it is running in lean mode.
                if (this.Languages.Contains(lstWords[1]) == false)
                {
                    this.Languages.LoadLocalizationFile(Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Localization"), lstWords[1]), lstWords[1]);
                }

                if (this.Languages.Contains(lstWords[1]) == true)
                {
                    this.CurrentLanguage = this.Languages[lstWords[1]];
                }
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.private.options.autoCheckDownloadUpdates", true) == 0 && objSender == this)
            {
                bool blEnabled = false;

                if (bool.TryParse(lstWords[1], out blEnabled) == true)
                {
                    this.OptionsSettings.AutoCheckDownloadUpdates = blEnabled;

                    // Force an update check right now..
                    if (this.OptionsSettings.AutoCheckDownloadUpdates == true)
                    {
                        //this.CheckVersion();
                        //this.VersionCheck("http://www.phogue.net/procon/version.php");
                    }
                }
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.private.options.autoApplyUpdates", true) == 0 && objSender == this)
            {
                bool blEnabled = false;

                if (bool.TryParse(lstWords[1], out blEnabled) == true)
                {
                    this.OptionsSettings.AutoApplyUpdates = blEnabled;
                }
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.private.options.autoCheckGameConfigsForUpdates", true) == 0 && objSender == this)
            {
                bool blEnabled = false;

                if (bool.TryParse(lstWords[1], out blEnabled) == true)
                {
                    this.OptionsSettings.AutoCheckGameConfigsForUpdates = blEnabled;
                }
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.private.options.allowanonymoususagedata", true) == 0 && objSender == this)
            {
                bool blEnabled = false;

                if (bool.TryParse(lstWords[1], out blEnabled) == true)
                {
                    this.OptionsSettings.AllowAnonymousUsageData = blEnabled;
                }
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.private.options.UseGeoIpFileOnly", true) == 0 && objSender == this)
            {
                bool blEnabled = false;

                if (bool.TryParse(lstWords[1], out blEnabled) == true)
                {
                    this.OptionsSettings.UseGeoIpFileOnly = blEnabled;
                }
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.private.options.BlockRssFeedNews", true) == 0 && objSender == this)
            {
                bool blEnabled = false;

                if (bool.TryParse(lstWords[1], out blEnabled) == true)
                {
                    this.OptionsSettings.BlockRssFeedNews = blEnabled;
                }
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.private.options.consoleLogging", true) == 0 && objSender == this)
            {
                bool blEnabled = false;

                if (bool.TryParse(lstWords[1], out blEnabled) == true)
                {
                    this.OptionsSettings.ConsoleLogging = blEnabled;
                }
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.private.options.eventsLogging", true) == 0 && objSender == this)
            {
                bool blEnabled = false;

                if (bool.TryParse(lstWords[1], out blEnabled) == true)
                {
                    this.OptionsSettings.EventsLogging = blEnabled;
                }
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.private.options.chatLogging", true) == 0 && objSender == this)
            {
                bool blEnabled = false;

                if (bool.TryParse(lstWords[1], out blEnabled) == true)
                {
                    this.OptionsSettings.ChatLogging = blEnabled;
                }
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.private.options.pluginLogging", true) == 0 && objSender == this)
            {
                bool blEnabled = false;

                if (bool.TryParse(lstWords[1], out blEnabled) == true)
                {
                    this.OptionsSettings.PluginLogging = blEnabled;
                }
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.private.options.showtrayicon", true) == 0 && objSender == this)
            {
                bool blEnabled = false;

                if (bool.TryParse(lstWords[1], out blEnabled) == true)
                {
                    this.OptionsSettings.ShowTrayIcon = blEnabled;
                }
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.private.options.minimizetotray", true) == 0 && objSender == this)
            {
                bool blEnabled = false;

                if (bool.TryParse(lstWords[1], out blEnabled) == true)
                {
                    this.OptionsSettings.MinimizeToTray = blEnabled;
                }
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.private.options.closetotray", true) == 0 && objSender == this)
            {
                bool blEnabled = false;

                if (bool.TryParse(lstWords[1], out blEnabled) == true)
                {
                    this.OptionsSettings.CloseToTray = blEnabled;
                }
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.private.options.runPluginsInSandbox", true) == 0 && objSender == this)
            {
                bool blEnabled = false;

                if (bool.TryParse(lstWords[1], out blEnabled) == true)
                {
                    this.OptionsSettings.RunPluginsInTrustedSandbox = blEnabled;
                }
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.private.options.adminMoveMessage", true) == 0 && objSender == this)
            {
                bool blEnabled = false;

                if (bool.TryParse(lstWords[1], out blEnabled) == true)
                {
                    this.OptionsSettings.AdminMoveMessage = blEnabled;
                }
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.private.options.chatDisplayAdminName", true) == 0 && objSender == this)
            {
                bool blEnabled = false;

                if (bool.TryParse(lstWords[1], out blEnabled) == true)
                {
                    this.OptionsSettings.ChatDisplayAdminName = blEnabled;
                }
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.private.options.EnableAdminReason", true) == 0 && objSender == this)
            {
                bool blEnabled = false;

                if (bool.TryParse(lstWords[1], out blEnabled) == true)
                {
                    this.OptionsSettings.EnableAdminReason = blEnabled;
                }
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.private.options.layerHideLocalPlugins", true) == 0 && objSender == this)
            {
                bool blEnabled = false;

                if (bool.TryParse(lstWords[1], out blEnabled) == true)
                {
                    this.OptionsSettings.LayerHideLocalPlugins = blEnabled;
                }
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.private.options.layerHideLocalAccounts", true) == 0 && objSender == this)
            {
                bool blEnabled = false;

                if (bool.TryParse(lstWords[1], out blEnabled) == true)
                {
                    this.OptionsSettings.LayerHideLocalAccounts = blEnabled;
                }
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.private.options.ShowRoundTimerConstantly", true) == 0 && objSender == this)
            {
                bool blEnabled = false;

                if (bool.TryParse(lstWords[1], out blEnabled) == true)
                {
                    this.OptionsSettings.ShowRoundTimerConstantly = blEnabled;
                }
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.private.options.ShowCfmMsgRoundRestartNext", true) == 0 && objSender == this)
            {
                bool blEnabled = false;

                if (bool.TryParse(lstWords[1], out blEnabled) == true)
                {
                    this.OptionsSettings.ShowCfmMsgRoundRestartNext = blEnabled;
                }
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.private.options.ShowDICESpecialOptions", true) == 0 && objSender == this)
            {
                bool blEnabled = false;

                if (bool.TryParse(lstWords[1], out blEnabled) == true)
                {
                    this.OptionsSettings.ShowDICESpecialOptions = blEnabled;
                }
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.private.options.allowAllODBCConnections", true) == 0 && objSender == this)
            {
                bool blEnabled = false;

                if (bool.TryParse(lstWords[1], out blEnabled) == true)
                {
                    this.OptionsSettings.AllowAllODBCConnections = blEnabled;
                }
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.private.options.allowAllSmtpConnections", true) == 0 && objSender == this)
            {
                bool blEnabled = false;

                if (bool.TryParse(lstWords[1], out blEnabled) == true)
                {
                    this.OptionsSettings.AllowAllSmtpConnections = blEnabled;
                }
            }
            else if (lstWords.Count >= 1 && String.Compare(lstWords[0], "procon.private.options.trustedHostDomainsPorts", true) == 0 && objSender == this)
            {

                lstWords.RemoveAt(0);

                UInt16 ui16Port = 0;
                for (int i = 0; i + 1 < lstWords.Count; i = i + 2)
                {
                    if (UInt16.TryParse(lstWords[i + 1], out ui16Port) == true)
                    {
                        this.OptionsSettings.TrustedHostsWebsitesPorts.Add(new TrustedHostWebsitePort(lstWords[i], ui16Port));
                    }
                }
            }
            else if (lstWords.Count >= 1 && String.Compare(lstWords[0], "procon.private.options.statsLinksMaxNum", true) == 0 && objSender == this)
            {
                int itmp = 4;
                if (int.TryParse(lstWords[1], out itmp) == true)
                {
                    this.OptionsSettings.StatsLinksMaxNum = itmp;
                }
            }
            else if (lstWords.Count >= 1 && String.Compare(lstWords[0], "procon.private.options.statsLinkNameUrl", true) == 0 && objSender == this)
            {
                this.OptionsSettings.StatsLinkNameUrl.Clear();
                lstWords.RemoveAt(0);
                for (int i = 0; i + 1 < lstWords.Count; i = i + 2)
                {
                    if (this.OptionsSettings.StatsLinkNameUrl.Count < this.OptionsSettings.StatsLinksMaxNum)
                    {
                        this.OptionsSettings.StatsLinkNameUrl.Add(new StatsLinkNameUrl(lstWords[i], lstWords[i + 1]));
                    }
                }
            }
            else if (lstWords.Count >= 1 && String.Compare(lstWords[0], "procon.private.options.pluginMaxRuntime", true) == 0 && objSender == this && this.OptionsSettings.PluginMaxRuntimeLocked == false)
            {
                this.OptionsSettings.PluginMaxRuntime_m = 0;
                this.OptionsSettings.PluginMaxRuntime_s = 10;
                lstWords.RemoveAt(0);

                int itmp = 0;
                if (lstWords.Count == 2)
                {
                    if (int.TryParse(lstWords[0], out itmp) == true)
                    {
                        if (itmp < 0) { itmp = 0; }
                        if (itmp >= 60) { itmp = 59; }
                        this.OptionsSettings.PluginMaxRuntime_m = itmp;
                    }
                    itmp = 10;
                    if (int.TryParse(lstWords[1], out itmp) == true)
                    {
                        if (itmp < 0) { itmp = 0; }
                        if (itmp >= 60) { itmp = 59; }
                        this.OptionsSettings.PluginMaxRuntime_s = itmp;
                    }
                }
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.private.options.UsePluginOldStyleLoad", true) == 0 && objSender == this)
            {
                bool blEnabled = false;

                if (bool.TryParse(lstWords[1], out blEnabled) == true)
                {
                    this.OptionsSettings.UsePluginOldStyleLoad = blEnabled;
                }
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.private.options.enablePluginDebugging", true) == 0 && objSender == this)
            {
                bool blEnabled = false;

                if (bool.TryParse(lstWords[1], out blEnabled) == true)
                {
                    this.OptionsSettings.EnablePluginDebugging = blEnabled;
                }
            }
            else if (lstWords.Count >= 3 && String.Compare(lstWords[0], "procon.protected.notification.write", true) == 0 && objSender is PRoConClient)
            {

                bool blError = false;

                if (lstWords.Count >= 4 && bool.TryParse(lstWords[3], out blError) == true)
                {
                    if (this.ShowNotification != null)
                    {
                        this.ShowNotification(2000, lstWords[1], lstWords[2], blError);
                    }
                }
                else
                {
                    if (this.ShowNotification != null)
                    {
                        this.ShowNotification(2000, lstWords[1], lstWords[2], false);
                    }
                }
            }

            else if (lstWords.Count >= 3 && String.Compare(lstWords[0], "procon.protected.playsound", true) == 0 && objSender is PRoConClient)
            {

                int iRepeat = 0;

                string blah = Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Media"), lstWords[1]);

                if (int.TryParse(lstWords[2], out iRepeat) == true && iRepeat > 0 && File.Exists(Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Media"), lstWords[1])) == true)
                {

                    //this.Invoke(new DispatchProconProtectedPlaySound(this.PlaySound), new object[] { lstWords[1], iRepeat });

                    ((PRoConClient)objSender).PlaySound(lstWords[1], iRepeat);
                }
            }
            else if (lstWords.Count >= 1 && String.Compare(lstWords[0], "procon.protected.stopsound", true) == 0 && objSender is PRoConClient)
            {
                //this.Invoke(new DispatchProconProtectedStopSound(this.StopSound), new object[] { default(SPlaySound) });
                ((PRoConClient)objSender).StopSound(default(PRoConClient.SPlaySound));
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.protected.events.captures", true) == 0 && objSender is PRoConClient)
            {
                lstWords.RemoveAt(0);
                ((PRoConClient)objSender).EventsLogging.Settings = lstWords;
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.protected.playerlist.settings", true) == 0 && objSender is PRoConClient)
            {
                lstWords.RemoveAt(0);
                ((PRoConClient)objSender).PlayerListSettings.Settings = lstWords;
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.protected.chat.settings", true) == 0 && objSender is PRoConClient)
            {
                lstWords.RemoveAt(0);
                ((PRoConClient)objSender).ChatConsole.Settings = lstWords;
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.protected.lists.settings", true) == 0 && objSender is PRoConClient)
            {
                lstWords.RemoveAt(0);
                ((PRoConClient)objSender).ListSettings.Settings = lstWords;
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.protected.console.settings", true) == 0 && objSender is PRoConClient)
            {
                lstWords.RemoveAt(0);
                ((PRoConClient)objSender).Console.Settings = lstWords;
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.protected.timezone_UTCoffset", true) == 0 && objSender is PRoConClient)
            {
                double UTCoffset;
                if (double.TryParse(lstWords[1], out UTCoffset) == true)
                {
                    ((PRoConClient)objSender).Game.UtcOffset = UTCoffset;
                }
                else
                {
                    ((PRoConClient)objSender).Game.UtcOffset = 0;
                }
            }
            else if (lstWords.Count >= 1 && String.Compare(lstWords[0], "procon.protected.tasks.clear", true) == 0 && objSender is PRoConClient)
            {
                ((PRoConClient)objSender).ProconProtectedTasksClear();
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.protected.tasks.remove", true) == 0 && objSender is PRoConClient)
            {
                ((PRoConClient)objSender).ProconProtectedTasksRemove(lstWords[1]);
            }
            else if (lstWords.Count >= 1 && String.Compare(lstWords[0], "procon.protected.tasks.list", true) == 0 && objSender is PRoConClient)
            {

                ((PRoConClient)objSender).ProconProtectedTasksList();
            }
            else if (lstWords.Count >= 4 && String.Compare(lstWords[0], "procon.protected.tasks.add", true) == 0 && objSender is PRoConClient)
            {

                int iDelay = 0, iInterval = 1, iRepeat = -1;
                string strTaskName = String.Empty;

                if (int.TryParse(lstWords[1], out iDelay) == true && int.TryParse(lstWords[2], out iInterval) == true && int.TryParse(lstWords[3], out iRepeat) == true)
                {

                    lstWords.RemoveRange(0, 4);
                    ((PRoConClient)objSender).ProconProtectedTasksAdd(String.Empty, lstWords, iDelay, iInterval, iRepeat);
                }
                else if (lstWords.Count >= 5 && int.TryParse(lstWords[2], out iDelay) == true && int.TryParse(lstWords[3], out iInterval) == true && int.TryParse(lstWords[4], out iRepeat) == true)
                {
                    strTaskName = lstWords[1];
                    lstWords.RemoveRange(0, 5);
                    ((PRoConClient)objSender).ProconProtectedTasksAdd(strTaskName, lstWords, iDelay, iInterval, iRepeat);
                }
            }
            else if (lstWords.Count >= 1 && String.Compare(lstWords[0], "procon.protected.vars.list", true) == 0 && objSender is PRoConClient)
            {

                ((PRoConClient)objSender).Console.Write("Local Variables: [Variable] [Value]");

                foreach (Variable kvpVariable in ((PRoConClient)objSender).Variables)
                {
                    ((PRoConClient)objSender).Console.Write(String.Format("{0} \"{1}\"", kvpVariable.Name, kvpVariable.Value));
                }

                ((PRoConClient)objSender).Console.Write(String.Format("End of Local Variables List ({0} Variables)", ((PRoConClient)objSender).Variables.Count));
            }
            else if (lstWords.Count >= 1 && String.Compare(lstWords[0], "procon.protected.sv_vars.list", true) == 0 && objSender is PRoConClient)
            {

                ((PRoConClient)objSender).Console.Write("Server Variables: [Variable] [Value]");

                foreach (Variable kvpVariable in ((PRoConClient)objSender).SV_Variables)
                {
                    ((PRoConClient)objSender).Console.Write(String.Format("{0} \"{1}\"", kvpVariable.Name, kvpVariable.Value));
                }

                ((PRoConClient)objSender).Console.Write(String.Format("End of Server Variables List ({0} Variables)", ((PRoConClient)objSender).SV_Variables.Count));
            }
            else if (lstWords.Count >= 3 && String.Compare(lstWords[0], "procon.protected.plugins.call", true) == 0 && objSender is PRoConClient)
            {

                if (((PRoConClient)objSender).PluginsManager != null)
                {
                    if (((PRoConClient)objSender).PluginsManager.Plugins.LoadedClassNames.Contains(lstWords[1]) == true)
                    {

                        string[] strParams = null;

                        if (lstWords.Count - 3 > 0)
                        {
                            strParams = new string[lstWords.Count - 3];
                            lstWords.CopyTo(3, strParams, 0, lstWords.Count - 3);
                        }

                        ((PRoConClient)objSender).PluginsManager.InvokeOnEnabled(lstWords[1], lstWords[2], strParams);
                    }
                }
            }
            else if (lstWords.Count >= 6 && String.Compare(lstWords[0], "procon.private.tcadmin.enableLayer", true) == 0 && objSender == this)
            {

                if (this.Connections.Contains(String.Format("{0}:{1}", lstWords[1], lstWords[2])) == true)
                {
                    UInt16 ui16Port = 0;
                    UInt16.TryParse(lstWords[4], out ui16Port);

                    this.Connections[String.Format("{0}:{1}", lstWords[1], lstWords[2])].ProconProtectedLayerEnable(true, ui16Port, lstWords[3], lstWords[5]);
                }
            }
            else if (lstWords.Count >= 5 && String.Compare(lstWords[0], "procon.private.tcadmin.setPrivileges", true) == 0 && objSender == this)
            {

                if (this.Connections.Contains(String.Format("{0}:{1}", lstWords[1], lstWords[2])) == true)
                {

                    CPrivileges sprPrivs = new CPrivileges();
                    UInt32 ui32Privileges = 0;

                    if (UInt32.TryParse(lstWords[4], out ui32Privileges) == true && this.AccountsList.Contains(lstWords[3]) == true)
                    {
                        sprPrivs.PrivilegesFlags = ui32Privileges;
                        this.Connections[String.Format("{0}:{1}", lstWords[1], lstWords[2])].ProconProtectedLayerSetPrivileges(this.AccountsList[lstWords[3]], sprPrivs);
                    }
                }
            }
        }

        public void ExecutePRoConCommandCon(object objSender, List<string> lstWords, int iRecursion)
        {

            if (lstWords.Count >= 4 && String.Compare(lstWords[0], "procon.protected.weapons.add", true) == 0 && objSender is PRoConClient)
            {

                if (((PRoConClient)objSender).Weapons.Contains(lstWords[2]) == false &&
                    Enum.IsDefined(typeof(Kits), lstWords[1]) == true &&
                    Enum.IsDefined(typeof(WeaponSlots), lstWords[3]) == true &&
                    Enum.IsDefined(typeof(DamageTypes), lstWords[4]) == true)
                {
                    //this.SavedWindowState = (System.Windows.Forms.FormWindowState)Enum.Parse(typeof(System.Windows.Forms.FormWindowState), lstWords[1]);

                    ((PRoConClient)objSender).Weapons.Add(
                            new Weapon(
                                (Kits)Enum.Parse(typeof(Kits), lstWords[1]),
                                lstWords[2],
                                (WeaponSlots)Enum.Parse(typeof(WeaponSlots), lstWords[3]),
                                (DamageTypes)Enum.Parse(typeof(DamageTypes), lstWords[4])
                            )
                        );
                }
            }
            else if (lstWords.Count >= 1 && String.Compare(lstWords[0], "procon.protected.weapons.clear", true) == 0 && objSender is PRoConClient)
            {
                ((PRoConClient)objSender).Weapons.Clear();
            }
            else if (lstWords.Count >= 1 && String.Compare(lstWords[0], "procon.protected.zones.clear", true) == 0 && objSender is PRoConClient)
            {
                ((PRoConClient)objSender).MapGeometry.MapZones.Clear();
            }
            else if (lstWords.Count >= 4 && String.Compare(lstWords[0], "procon.protected.zones.add", true) == 0 && objSender is PRoConClient)
            {

                List<Point3D> points = new List<Point3D>();
                int iPoints = 0;

                if (int.TryParse(lstWords[4], out iPoints) == true)
                {

                    for (int i = 0, iOffset = 5; i < iPoints && iOffset + 3 <= lstWords.Count; i++)
                    {
                        points.Add(new Point3D(lstWords[iOffset++], lstWords[iOffset++], lstWords[iOffset++]));
                    }
                }

                if (((PRoConClient)objSender).MapGeometry.MapZones.Contains(lstWords[1]) == false)
                {
                    ((PRoConClient)objSender).MapGeometry.MapZones.Add(new MapZoneDrawing(lstWords[1], lstWords[2], lstWords[3], points.ToArray(), true));
                }
            }


            else if (lstWords.Count >= 3 && String.Compare(lstWords[0], "procon.protected.specialization.add", true) == 0 && objSender is PRoConClient)
            {

                if (((PRoConClient)objSender).Specializations.Contains(lstWords[2]) == false &&
                    Enum.IsDefined(typeof(SpecializationSlots), lstWords[1]) == true)
                {

                    ((PRoConClient)objSender).Specializations.Add(new Specialization((SpecializationSlots)Enum.Parse(typeof(SpecializationSlots), lstWords[1]), lstWords[2]));
                }

            }
            else if (lstWords.Count >= 1 && String.Compare(lstWords[0], "procon.protected.specialization.clear", true) == 0 && objSender is PRoConClient)
            {
                ((PRoConClient)objSender).Specializations.Clear();
            }
            else if (lstWords.Count >= 5 && String.Compare(lstWords[0], "procon.protected.teamnames.add", true) == 0 && objSender is PRoConClient)
            {

                if (lstWords.Count >= 6)
                {
                    int iTeamID = 0;
                    if (int.TryParse(lstWords[2], out iTeamID) == true)
                    {
                        ((PRoConClient)objSender).ProconProtectedTeamNamesAdd(lstWords[1], iTeamID, lstWords[3], lstWords[4], lstWords[5]);
                    }
                }
                else
                {
                    int iTeamID = 0;
                    if (int.TryParse(lstWords[2], out iTeamID) == true)
                    {
                        ((PRoConClient)objSender).ProconProtectedTeamNamesAdd(lstWords[1], iTeamID, lstWords[3], lstWords[4]);
                    }
                }
            }
            else if (lstWords.Count >= 6 && String.Compare(lstWords[0], "procon.protected.maps.add", true) == 0 && objSender is PRoConClient)
            {
                int iDefaultSquadID = 0;
                if (int.TryParse(lstWords[5], out iDefaultSquadID) == true)
                {
                    ((PRoConClient)objSender).ProconProtectedMapsAdd(lstWords[1], lstWords[2], lstWords[3], lstWords[4], iDefaultSquadID);
                }
            }
            else if (lstWords.Count >= 4 && String.Compare(lstWords[0], "procon.protected.plugins.setVariable", true) == 0 && objSender is PRoConClient)
            {

                string strUnescapedNewlines = lstWords[3].Replace(@"\n", "\n");
                strUnescapedNewlines = strUnescapedNewlines.Replace(@"\r", "\r");
                strUnescapedNewlines = strUnescapedNewlines.Replace(@"\""", "\"");

                ((PRoConClient)objSender).ProconProtectedPluginSetVariableCon(lstWords[1], lstWords[2], strUnescapedNewlines);
            }
            else if (lstWords.Count >= 3 && String.Compare(lstWords[0], "procon.protected.vars.set", true) == 0 && objSender is PRoConClient)
            {
                ((PRoConClient)objSender).Variables.SetVariable(lstWords[1], lstWords[2]);
            }
            else if (lstWords.Count >= 3 && String.Compare(lstWords[0], "procon.protected.layer.setPrivileges", true) == 0 && objSender is PRoConClient)
            {

                CPrivileges sprPrivs = new CPrivileges();
                UInt32 ui32Privileges = 0;

                if (UInt32.TryParse(lstWords[2], out ui32Privileges) == true)
                {
                    sprPrivs.PrivilegesFlags = ui32Privileges;
                    if (this.AccountsList.Contains(lstWords[1]) == true)
                    {
                        ((PRoConClient)objSender).ProconProtectedLayerSetPrivileges(this.AccountsList[lstWords[1]], sprPrivs);
                    }
                }
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.protected.send", true) == 0 && objSender is PRoConClient)
            {
                lstWords.RemoveAt(0);

                // Block them from changing the admin password to X
                if (String.Compare(lstWords[0], "vars.adminPassword", true) != 0)
                {
                    ((PRoConClient)objSender).SendRequest(lstWords);
                }
            }
            else if (lstWords.Count >= 3 && String.Compare(lstWords[0], "procon.public.accounts.create", true) == 0)
            {
                this.AccountsList.CreateAccount(lstWords[1], lstWords[2]);
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.public.accounts.delete", true) == 0)
            {
                this.AccountsList.DeleteAccount(lstWords[1]);
            }
            else if (lstWords.Count >= 3 && String.Compare(lstWords[0], "procon.public.accounts.setPassword", true) == 0)
            {
                this.AccountsList.ChangePassword(lstWords[1], lstWords[2]);
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.protected.config.exec", true) == 0 && objSender is PRoConClient)
            {
                if (iRecursion < 5)
                {
                    ((PRoConClient)objSender).ExecuteConnectionConfig(lstWords[1], iRecursion, lstWords.Count > 2 ? lstWords.GetRange(2, lstWords.Count - 2) : null, false);
                }
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.protected.pluginconsole.write", true) == 0 && objSender is PRoConClient)
            {
                ((PRoConClient)objSender).PluginConsole.Write(lstWords[1]);
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.protected.console.write", true) == 0 && objSender is PRoConClient)
            {
                ((PRoConClient)objSender).Console.Write(lstWords[1]);
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.protected.chat.write", true) == 0 && objSender is PRoConClient)
            {
                ((PRoConClient)objSender).ChatConsole.WriteViaCommand(lstWords[1]);
            }
            else if (lstWords.Count >= 5 && String.Compare(lstWords[0], "procon.protected.events.write", true) == 0 && objSender is PRoConClient)
            {

                // EventType etType, CapturableEvents ceEvent, string strEventText, DateTime dtLoggedTime, string instigatingAdmin

                if (Enum.IsDefined(typeof(EventType), lstWords[1]) == true && Enum.IsDefined(typeof(CapturableEvents), lstWords[2]) == true)
                {

                    EventType type = (EventType)Enum.Parse(typeof(EventType), lstWords[1]);
                    CapturableEvents cappedEventType = (CapturableEvents)Enum.Parse(typeof(CapturableEvents), lstWords[2]);

                    CapturedEvent cappedEvent = new CapturedEvent(type, cappedEventType, lstWords[3], DateTime.Now, lstWords[4]);

                    ((PRoConClient)objSender).EventsLogging.ProcessEvent(cappedEvent);
                }
            }

            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.protected.layer.enable", true) == 0 && objSender is PRoConClient)
            {
                // procon.protected.layer.enable <true> <port>
                bool blEnabled = false;
                UInt16 ui16Port = 0;

                if (bool.TryParse(lstWords[1], out blEnabled) == true)
                {

                    if (lstWords.Count >= 5)
                    {
                        UInt16.TryParse(lstWords[2], out ui16Port);
                        ((PRoConClient)objSender).ProconProtectedLayerEnable(blEnabled, ui16Port, lstWords[3], lstWords[4]);
                    }
                    else
                    {
                        ((PRoConClient)objSender).ProconProtectedLayerEnable(blEnabled, 27260, "0.0.0.0", "PRoCon[%servername%]");
                    }
                }
            }
            else if (lstWords.Count >= 1 && String.Compare(lstWords[0], "procon.protected.teamnames.clear", true) == 0 && objSender is PRoConClient)
            {
                ((PRoConClient)objSender).ProconProtectedTeamNamesClear();
            }
            else if (lstWords.Count >= 1 && String.Compare(lstWords[0], "procon.protected.maps.clear", true) == 0 && objSender is PRoConClient)
            {
                ((PRoConClient)objSender).ProconProtectedMapsClear();
            }
            else if (lstWords.Count >= 1 && String.Compare(lstWords[0], "procon.protected.reasons.clear", true) == 0 && objSender is PRoConClient)
            {
                ((PRoConClient)objSender).ProconProtectedReasonsClear();
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.protected.reasons.add", true) == 0 && objSender is PRoConClient)
            {
                ((PRoConClient)objSender).ProconProtectedReasonsAdd(lstWords[1]);
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.protected.serverversions.clear", true) == 0 && objSender is PRoConClient)
            {
                ((PRoConClient)objSender).ProconProtectedServerVersionsClear(lstWords[1]);
            }
            else if (lstWords.Count >= 4 && String.Compare(lstWords[0], "procon.protected.serverversions.add", true) == 0 && objSender is PRoConClient)
            {
                ((PRoConClient)objSender).ProconProtectedServerVersionsAdd(lstWords[1], lstWords[2], lstWords[3]);
            }
            else if (lstWords.Count >= 3 && String.Compare(lstWords[0], "procon.protected.plugins.enable", true) == 0 && objSender is PRoConClient)
            {

                bool blEnabled = false;

                if (bool.TryParse(lstWords[2], out blEnabled) == true)
                {
                    ((PRoConClient)objSender).ProconProtectedPluginEnable(lstWords[1], blEnabled);
                }
            }
            else if (lstWords.Count >= 3 && String.Compare(lstWords[0], "procon.private.servers.add", true) == 0 && objSender == this)
            {
                // add IP port [password [username]]
                // procon.private.servers.add "127.0.0.1" 27260 "Password" "Phogue"
                UInt16 ui16Port = 0;
                if (UInt16.TryParse(lstWords[2], out ui16Port) == true)
                {
                    if (lstWords.Count == 3)
                    {
                        this.AddConnection(lstWords[1], ui16Port, String.Empty, String.Empty);
                    }
                    else if (lstWords.Count == 4)
                    {
                        this.AddConnection(lstWords[1], ui16Port, String.Empty, lstWords[3]);
                    }
                    else if (lstWords.Count == 5)
                    {
                        this.AddConnection(lstWords[1], ui16Port, lstWords[4], lstWords[3]);
                    }
                }
            }
            else if (lstWords.Count >= 3 && String.Compare(lstWords[0], "procon.private.servers.connect", true) == 0 && objSender == this)
            {

                if (this.Connections.Contains(lstWords[1] + ":" + lstWords[2]) == true)
                {
                    this.Connections[lstWords[1] + ":" + lstWords[2]].ProconPrivateServerConnect();
                }
            }
            else if (lstWords.Count >= 3 && String.Compare(lstWords[0], "procon.private.servers.autoconnect", true) == 0 && objSender == this)
            {
                if (this.Connections.Contains(lstWords[1] + ":" + lstWords[2]) == true)
                {

                    this.Connections[lstWords[1] + ":" + lstWords[2]].AutomaticallyConnect = true;

                    // Originally leaving it for the reconnect thread to pickup but needed a quicker effect.
                    this.Connections[lstWords[1] + ":" + lstWords[2]].ProconPrivateServerConnect();
                }
            }

            else if (lstWords.Count >= 3 && String.Compare(lstWords[0], "procon.private.servers.name", true) == 0 && objSender == this)
            {
                // CurrentServerInfo not initialized.
                if (this.Connections.Contains(lstWords[1] + ":" + lstWords[2]) == true)
                {
                    this.Connections[lstWords[1] + ":" + lstWords[2]].ConnectionServerName = lstWords[3];
                }
            }

            /*
            else if (lstWords.Count >= 3 && String.Compare(lstWords[0], "procon.private.servers.name", true) == 0 && objSender == this) {
                this.uscServerPlayerTreeviewListing.SetServerName(lstWords[1] + ":" + lstWords[2], lstWords[3]);
            }

            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.private.window.splitterPosition", true) == 0 && objSender == this) {
                int iPositionVar = 0;

                if (int.TryParse(lstWords[1], out iPositionVar) == true) {
                    if (iPositionVar >= this.spltTreeServers.Panel1MinSize && iPositionVar <= this.spltTreeServers.Width - this.spltTreeServers.Panel2MinSize) {
                        this.spltTreeServers.SplitterDistance = iPositionVar;
                    }
                }
            }
            */

            else if (lstWords.Count >= 6 && String.Compare(lstWords[0], "procon.private.window.position", true) == 0 && objSender == this)
            {

                Rectangle recWindowBounds = new Rectangle(0, 0, 1024, 768);
                int iPositionVar = 0;

                if (Enum.IsDefined(typeof(System.Windows.Forms.FormWindowState), lstWords[1]) == true)
                {
                    this.SavedWindowState = (System.Windows.Forms.FormWindowState)Enum.Parse(typeof(System.Windows.Forms.FormWindowState), lstWords[1]);

                    if (int.TryParse(lstWords[2], out iPositionVar) == true)
                    {
                        if (iPositionVar >= 0)
                        {
                            recWindowBounds.X = iPositionVar;
                        }
                    }

                    if (int.TryParse(lstWords[3], out iPositionVar) == true)
                    {
                        if (iPositionVar >= 0)
                        {
                            recWindowBounds.Y = iPositionVar;
                        }
                    }

                    if (int.TryParse(lstWords[4], out iPositionVar) == true)
                    {
                        recWindowBounds.Width = iPositionVar;
                    }

                    if (int.TryParse(lstWords[5], out iPositionVar) == true)
                    {
                        recWindowBounds.Height = iPositionVar;
                    }

                    this.SavedWindowBounds = recWindowBounds;
                }

            }







            // procon.private.httpWebServer.enable true 27360 "0.0.0.0"
            else if (lstWords.Count >= 4 && String.Compare(lstWords[0], "procon.private.httpWebServer.enable", true) == 0 && objSender == this)
            {

                bool blEnabled = false;
                string bindingAddress = "0.0.0.0";
                UInt16 ui16Port = 27360;

                if (bool.TryParse(lstWords[1], out blEnabled) == true)
                {

                    if (this.HttpWebServer != null)
                    {
                        this.HttpWebServer.Shutdown();

                        this.HttpWebServer.ProcessRequest -= new HttpWebServer.ProcessResponseHandler(HttpWebServer_ProcessRequest);
                        this.HttpWebServer.HttpServerOnline -= new HttpWebServer.StateChangeHandler(HttpWebServer_HttpServerOnline);
                        this.HttpWebServer.HttpServerOffline -= new HttpWebServer.StateChangeHandler(HttpWebServer_HttpServerOffline);
                    }

                    bindingAddress = lstWords[3];
                    if (UInt16.TryParse(lstWords[2], out ui16Port) == false)
                    {
                        ui16Port = 27360;
                    }

                    this.HttpWebServer = new HttpWebServer(bindingAddress, ui16Port);
                    this.HttpWebServer.ProcessRequest += new HttpWebServer.ProcessResponseHandler(HttpWebServer_ProcessRequest);
                    this.HttpWebServer.HttpServerOnline += new HttpWebServer.StateChangeHandler(HttpWebServer_HttpServerOnline);
                    this.HttpWebServer.HttpServerOffline += new HttpWebServer.StateChangeHandler(HttpWebServer_HttpServerOffline);

                    if (blEnabled == true)
                    {
                        this.HttpWebServer.Start();
                    }
                }
            }

            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.private.options.setLanguage", true) == 0 && objSender == this)
            {

                // if it does not exist but they have explicity asked for it, see if we can load it up
                // this could not be loaded because it is running in lean mode.
                if (this.Languages.Contains(lstWords[1]) == false)
                {
                    this.Languages.LoadLocalizationFile(Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Localization"), lstWords[1]), lstWords[1]);
                }

                if (this.Languages.Contains(lstWords[1]) == true)
                {
                    this.CurrentLanguage = this.Languages[lstWords[1]];
                }
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.private.options.autoCheckDownloadUpdates", true) == 0 && objSender == this)
            {
                bool blEnabled = false;

                if (bool.TryParse(lstWords[1], out blEnabled) == true)
                {
                    this.OptionsSettings.AutoCheckDownloadUpdates = blEnabled;

                    // Force an update check right now..
                    if (this.OptionsSettings.AutoCheckDownloadUpdates == true)
                    {
                        //this.CheckVersion();
                        //this.VersionCheck("http://www.phogue.net/procon/version.php");
                    }
                }
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.private.options.autoApplyUpdates", true) == 0 && objSender == this)
            {
                bool blEnabled = false;

                if (bool.TryParse(lstWords[1], out blEnabled) == true)
                {
                    this.OptionsSettings.AutoApplyUpdates = blEnabled;
                }
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.private.options.autoCheckGameConfigsForUpdates", true) == 0 && objSender == this)
            {
                bool blEnabled = false;

                if (bool.TryParse(lstWords[1], out blEnabled) == true)
                {
                    this.OptionsSettings.AutoCheckGameConfigsForUpdates = blEnabled;
                }
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.private.options.allowanonymoususagedata", true) == 0 && objSender == this)
            {
                bool blEnabled = false;

                if (bool.TryParse(lstWords[1], out blEnabled) == true)
                {
                    this.OptionsSettings.AllowAnonymousUsageData = blEnabled;
                }
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.private.options.consoleLogging", true) == 0 && objSender == this)
            {
                bool blEnabled = false;

                if (bool.TryParse(lstWords[1], out blEnabled) == true)
                {
                    this.OptionsSettings.ConsoleLogging = blEnabled;
                }
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.private.options.eventsLogging", true) == 0 && objSender == this)
            {
                bool blEnabled = false;

                if (bool.TryParse(lstWords[1], out blEnabled) == true)
                {
                    this.OptionsSettings.EventsLogging = blEnabled;
                }
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.private.options.chatLogging", true) == 0 && objSender == this)
            {
                bool blEnabled = false;

                if (bool.TryParse(lstWords[1], out blEnabled) == true)
                {
                    this.OptionsSettings.ChatLogging = blEnabled;
                }
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.private.options.pluginLogging", true) == 0 && objSender == this)
            {
                bool blEnabled = false;

                if (bool.TryParse(lstWords[1], out blEnabled) == true)
                {
                    this.OptionsSettings.PluginLogging = blEnabled;
                }
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.private.options.showtrayicon", true) == 0 && objSender == this)
            {
                bool blEnabled = false;

                if (bool.TryParse(lstWords[1], out blEnabled) == true)
                {
                    this.OptionsSettings.ShowTrayIcon = blEnabled;
                }
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.private.options.minimizetotray", true) == 0 && objSender == this)
            {
                bool blEnabled = false;

                if (bool.TryParse(lstWords[1], out blEnabled) == true)
                {
                    this.OptionsSettings.MinimizeToTray = blEnabled;
                }
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.private.options.closetotray", true) == 0 && objSender == this)
            {
                bool blEnabled = false;

                if (bool.TryParse(lstWords[1], out blEnabled) == true)
                {
                    this.OptionsSettings.CloseToTray = blEnabled;
                }
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.private.options.runPluginsInSandbox", true) == 0 && objSender == this)
            {
                bool blEnabled = false;

                if (bool.TryParse(lstWords[1], out blEnabled) == true)
                {
                    this.OptionsSettings.RunPluginsInTrustedSandbox = blEnabled;
                }
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.private.options.adminMoveMessage", true) == 0 && objSender == this)
            {
                bool blEnabled = false;

                if (bool.TryParse(lstWords[1], out blEnabled) == true)
                {
                    this.OptionsSettings.AdminMoveMessage = blEnabled;
                }
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.private.options.chatDisplayAdminName", true) == 0 && objSender == this)
            {
                bool blEnabled = false;

                if (bool.TryParse(lstWords[1], out blEnabled) == true)
                {
                    this.OptionsSettings.ChatDisplayAdminName = blEnabled;
                }
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.private.options.EnableAdminReason", true) == 0 && objSender == this)
            {
                bool blEnabled = false;

                if (bool.TryParse(lstWords[1], out blEnabled) == true)
                {
                    this.OptionsSettings.EnableAdminReason = blEnabled;
                }
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.private.options.layerHideLocalPlugins", true) == 0 && objSender == this)
            {
                bool blEnabled = false;

                if (bool.TryParse(lstWords[1], out blEnabled) == true)
                {
                    this.OptionsSettings.LayerHideLocalPlugins = blEnabled;
                }
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.private.options.layerHideLocalAccounts", true) == 0 && objSender == this)
            {
                bool blEnabled = false;

                if (bool.TryParse(lstWords[1], out blEnabled) == true)
                {
                    this.OptionsSettings.LayerHideLocalAccounts = blEnabled;
                }
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.private.options.ShowRoundTimerConstantly", true) == 0 && objSender == this)
            {
                bool blEnabled = false;

                if (bool.TryParse(lstWords[1], out blEnabled) == true)
                {
                    this.OptionsSettings.ShowRoundTimerConstantly = blEnabled;
                }
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.private.options.ShowCfmMsgRoundRestartNext", true) == 0 && objSender == this)
            {
                bool blEnabled = false;

                if (bool.TryParse(lstWords[1], out blEnabled) == true)
                {
                    this.OptionsSettings.ShowCfmMsgRoundRestartNext = blEnabled;
                }
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.private.options.ShowDICESpecialOptions", true) == 0 && objSender == this)
            {
                bool blEnabled = false;

                if (bool.TryParse(lstWords[1], out blEnabled) == true)
                {
                    this.OptionsSettings.ShowDICESpecialOptions = blEnabled;
                }
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.private.options.allowAllODBCConnections", true) == 0 && objSender == this)
            {
                bool blEnabled = false;

                if (bool.TryParse(lstWords[1], out blEnabled) == true)
                {
                    this.OptionsSettings.AllowAllODBCConnections = blEnabled;
                }
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.private.options.allowAllSmtpConnections", true) == 0 && objSender == this)
            {
                bool blEnabled = false;

                if (bool.TryParse(lstWords[1], out blEnabled) == true)
                {
                    this.OptionsSettings.AllowAllSmtpConnections = blEnabled;
                }
            }
            else if (lstWords.Count >= 1 && String.Compare(lstWords[0], "procon.private.options.trustedHostDomainsPorts", true) == 0 && objSender == this)
            {

                lstWords.RemoveAt(0);

                UInt16 ui16Port = 0;
                for (int i = 0; i + 1 < lstWords.Count; i = i + 2)
                {
                    if (UInt16.TryParse(lstWords[i + 1], out ui16Port) == true)
                    {
                        this.OptionsSettings.TrustedHostsWebsitesPorts.Add(new TrustedHostWebsitePort(lstWords[i], ui16Port));
                    }
                }
            }
            else if (lstWords.Count >= 1 && String.Compare(lstWords[0], "procon.private.options.statsLinksMaxNum", true) == 0 && objSender == this)
            {
                int itmp = 4;
                if (int.TryParse(lstWords[1], out itmp) == true)
                {
                    this.OptionsSettings.StatsLinksMaxNum = itmp;
                }
            }
            else if (lstWords.Count >= 1 && String.Compare(lstWords[0], "procon.private.options.statsLinkNameUrl", true) == 0 && objSender == this)
            {
                this.OptionsSettings.StatsLinkNameUrl.Clear();
                lstWords.RemoveAt(0);
                for (int i = 0; i + 1 < lstWords.Count; i = i + 2)
                {
                    if (this.OptionsSettings.StatsLinkNameUrl.Count < this.OptionsSettings.StatsLinksMaxNum)
                    {
                        this.OptionsSettings.StatsLinkNameUrl.Add(new StatsLinkNameUrl(lstWords[i], lstWords[i + 1]));
                    }
                }
            }
            else if (lstWords.Count >= 3 && String.Compare(lstWords[0], "procon.protected.notification.write", true) == 0 && objSender is PRoConClient)
            {

                bool blError = false;

                if (lstWords.Count >= 4 && bool.TryParse(lstWords[3], out blError) == true)
                {
                    if (this.ShowNotification != null)
                    {
                        this.ShowNotification(2000, lstWords[1], lstWords[2], blError);
                    }
                }
                else
                {
                    if (this.ShowNotification != null)
                    {
                        this.ShowNotification(2000, lstWords[1], lstWords[2], false);
                    }
                }
            }

            else if (lstWords.Count >= 3 && String.Compare(lstWords[0], "procon.protected.playsound", true) == 0 && objSender is PRoConClient)
            {

                int iRepeat = 0;

                string blah = Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Media"), lstWords[1]);

                if (int.TryParse(lstWords[2], out iRepeat) == true && iRepeat > 0 && File.Exists(Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Media"), lstWords[1])) == true)
                {

                    //this.Invoke(new DispatchProconProtectedPlaySound(this.PlaySound), new object[] { lstWords[1], iRepeat });

                    ((PRoConClient)objSender).PlaySound(lstWords[1], iRepeat);
                }
            }
            else if (lstWords.Count >= 1 && String.Compare(lstWords[0], "procon.protected.stopsound", true) == 0 && objSender is PRoConClient)
            {
                //this.Invoke(new DispatchProconProtectedStopSound(this.StopSound), new object[] { default(SPlaySound) });
                ((PRoConClient)objSender).StopSound(default(PRoConClient.SPlaySound));
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.protected.events.captures", true) == 0 && objSender is PRoConClient)
            {
                lstWords.RemoveAt(0);
                ((PRoConClient)objSender).EventsLogging.Settings = lstWords;
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.protected.playerlist.settings", true) == 0 && objSender is PRoConClient)
            {
                lstWords.RemoveAt(0);
                ((PRoConClient)objSender).PlayerListSettings.Settings = lstWords;
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.protected.chat.settings", true) == 0 && objSender is PRoConClient)
            {
                lstWords.RemoveAt(0);
                ((PRoConClient)objSender).ChatConsole.Settings = lstWords;
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.protected.lists.settings", true) == 0 && objSender is PRoConClient)
            {
                lstWords.RemoveAt(0);
                ((PRoConClient)objSender).ListSettings.Settings = lstWords;
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.protected.console.settings", true) == 0 && objSender is PRoConClient)
            {
                lstWords.RemoveAt(0);
                ((PRoConClient)objSender).Console.Settings = lstWords;
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.protected.timezone_UTCoffset", true) == 0 && objSender is PRoConClient)
            {
                double UTCoffset;
                if (double.TryParse(lstWords[1], out UTCoffset) == true)
                {
                    ((PRoConClient)objSender).Game.UtcOffset = UTCoffset;
                }
                else
                {
                    ((PRoConClient)objSender).Game.UtcOffset = 0;
                }
            }
            else if (lstWords.Count >= 1 && String.Compare(lstWords[0], "procon.protected.tasks.clear", true) == 0 && objSender is PRoConClient)
            {
                ((PRoConClient)objSender).ProconProtectedTasksClear();
            }
            else if (lstWords.Count >= 2 && String.Compare(lstWords[0], "procon.protected.tasks.remove", true) == 0 && objSender is PRoConClient)
            {
                ((PRoConClient)objSender).ProconProtectedTasksRemove(lstWords[1]);
            }
            else if (lstWords.Count >= 1 && String.Compare(lstWords[0], "procon.protected.tasks.list", true) == 0 && objSender is PRoConClient)
            {

                ((PRoConClient)objSender).ProconProtectedTasksList();
            }
            else if (lstWords.Count >= 4 && String.Compare(lstWords[0], "procon.protected.tasks.add", true) == 0 && objSender is PRoConClient)
            {

                int iDelay = 0, iInterval = 1, iRepeat = -1;
                string strTaskName = String.Empty;

                if (int.TryParse(lstWords[1], out iDelay) == true && int.TryParse(lstWords[2], out iInterval) == true && int.TryParse(lstWords[3], out iRepeat) == true)
                {

                    lstWords.RemoveRange(0, 4);
                    ((PRoConClient)objSender).ProconProtectedTasksAdd(String.Empty, lstWords, iDelay, iInterval, iRepeat);
                }
                else if (lstWords.Count >= 5 && int.TryParse(lstWords[2], out iDelay) == true && int.TryParse(lstWords[3], out iInterval) == true && int.TryParse(lstWords[4], out iRepeat) == true)
                {
                    strTaskName = lstWords[1];
                    lstWords.RemoveRange(0, 5);
                    ((PRoConClient)objSender).ProconProtectedTasksAdd(strTaskName, lstWords, iDelay, iInterval, iRepeat);
                }
            }
            else if (lstWords.Count >= 1 && String.Compare(lstWords[0], "procon.protected.vars.list", true) == 0 && objSender is PRoConClient)
            {

                ((PRoConClient)objSender).Console.Write("Local Variables: [Variable] [Value]");

                foreach (Variable kvpVariable in ((PRoConClient)objSender).Variables)
                {
                    ((PRoConClient)objSender).Console.Write(String.Format("{0} \"{1}\"", kvpVariable.Name, kvpVariable.Value));
                }

                ((PRoConClient)objSender).Console.Write(String.Format("End of Local Variables List ({0} Variables)", ((PRoConClient)objSender).Variables.Count));
            }
            else if (lstWords.Count >= 1 && String.Compare(lstWords[0], "procon.protected.sv_vars.list", true) == 0 && objSender is PRoConClient)
            {

                ((PRoConClient)objSender).Console.Write("Server Variables: [Variable] [Value]");

                foreach (Variable kvpVariable in ((PRoConClient)objSender).SV_Variables)
                {
                    ((PRoConClient)objSender).Console.Write(String.Format("{0} \"{1}\"", kvpVariable.Name, kvpVariable.Value));
                }

                ((PRoConClient)objSender).Console.Write(String.Format("End of Server Variables List ({0} Variables)", ((PRoConClient)objSender).SV_Variables.Count));
            }
            else if (lstWords.Count >= 3 && String.Compare(lstWords[0], "procon.protected.plugins.call", true) == 0 && objSender is PRoConClient)
            {

                if (((PRoConClient)objSender).PluginsManager != null)
                {
                    if (((PRoConClient)objSender).PluginsManager.Plugins.LoadedClassNames.Contains(lstWords[1]) == true)
                    {

                        string[] strParams = null;

                        if (lstWords.Count - 3 > 0)
                        {
                            strParams = new string[lstWords.Count - 3];
                            lstWords.CopyTo(3, strParams, 0, lstWords.Count - 3);
                        }

                        ((PRoConClient)objSender).PluginsManager.InvokeOnEnabled(lstWords[1], lstWords[2], strParams);
                    }
                }
            }
            else if (lstWords.Count >= 6 && String.Compare(lstWords[0], "procon.private.tcadmin.enableLayer", true) == 0 && objSender == this)
            {

                if (this.Connections.Contains(String.Format("{0}:{1}", lstWords[1], lstWords[2])) == true)
                {
                    UInt16 ui16Port = 0;
                    UInt16.TryParse(lstWords[4], out ui16Port);

                    this.Connections[String.Format("{0}:{1}", lstWords[1], lstWords[2])].ProconProtectedLayerEnable(true, ui16Port, lstWords[3], lstWords[5]);
                }
            }
            else if (lstWords.Count >= 5 && String.Compare(lstWords[0], "procon.private.tcadmin.setPrivileges", true) == 0 && objSender == this)
            {

                if (this.Connections.Contains(String.Format("{0}:{1}", lstWords[1], lstWords[2])) == true)
                {

                    CPrivileges sprPrivs = new CPrivileges();
                    UInt32 ui32Privileges = 0;

                    if (UInt32.TryParse(lstWords[4], out ui32Privileges) == true && this.AccountsList.Contains(lstWords[3]) == true)
                    {
                        sprPrivs.PrivilegesFlags = ui32Privileges;
                        this.Connections[String.Format("{0}:{1}", lstWords[1], lstWords[2])].ProconProtectedLayerSetPrivileges(this.AccountsList[lstWords[3]], sprPrivs);
                    }
                }
            }
        }

        private void HttpWebServer_HttpServerOffline(HttpWebServer sender)
        {
            if (this.HttpServerOffline != null)
            {
                this.HttpServerOffline(sender);
            }
        }

        private void HttpWebServer_HttpServerOnline(HttpWebServer sender)
        {
            if (this.HttpServerOnline != null)
            {
                this.HttpServerOnline(sender);
            }
        }

        #endregion

        #region RSS Feed

        public void UpdateRss()
        {
			if (this.OptionsSettings.BlockRssFeedNews == true)
			{
				// Begin RSS Update
				if (this.BeginRssUpdate != null)
				{
					this.BeginRssUpdate(this);
				}

				CDownloadFile downloadRssFeed = new CDownloadFile("https://myrcon.net/rss/1-procon-news.xml/");
				downloadRssFeed.DownloadComplete += new CDownloadFile.DownloadFileEventDelegate(downloadRssFeed_DownloadComplete);
				downloadRssFeed.DownloadError += new CDownloadFile.DownloadFileEventDelegate(downloadRssFeed_DownloadError);
				downloadRssFeed.BeginDownload();

				CDownloadFile downloadPromoFeed = new CDownloadFile("https://myrcon.com/procon/streams/banners/format/xml");
				downloadPromoFeed.DownloadComplete += new CDownloadFile.DownloadFileEventDelegate(downloadPromoFeed_DownloadComplete);
				downloadPromoFeed.DownloadError += new CDownloadFile.DownloadFileEventDelegate(downloadPromoFeed_DownloadError);
				downloadPromoFeed.BeginDownload();
			}
        }

        private void downloadRssFeed_DownloadComplete(CDownloadFile cdfSender)
        {

            string xmlDocumentText = Encoding.UTF8.GetString(cdfSender.CompleteFileData);

            XmlDocument rssDocument = new XmlDocument();

            try
            {
                rssDocument.LoadXml(xmlDocumentText);

                /* not used anymore
                if (this.PackageManager != null) {
                    this.PackageManager.LoadRemotePackages(rssDocument);
                }
                */
                if (this.RssUpdateSuccess != null)
                {
                    this.RssUpdateSuccess(this, rssDocument);
                }
            }
            catch (Exception) { }

        }

        private void downloadRssFeed_DownloadError(CDownloadFile cdfSender)
        {

            // RSS Error
            if (this.RssUpdateError != null)
            {
                this.RssUpdateError(this);
            }

        }

        private void downloadPromoFeed_DownloadComplete(CDownloadFile cdfSender)
        {

            string xmlDocumentText = Encoding.UTF8.GetString(cdfSender.CompleteFileData);

            XmlDocument rssDocument = new XmlDocument();

            try
            {
                rssDocument.LoadXml(xmlDocumentText);

                if (this.RssUpdateSuccess != null)
                {
                    this.PromoUpdateSuccess(this, rssDocument);
                }
            }
            catch (Exception) { }

        }

        private void downloadPromoFeed_DownloadError(CDownloadFile cdfSender)
        {

            // RSS Error
            if (this.RssUpdateError != null)
            {
                this.PromoUpdateError(this);
            }

        }

        #endregion

        #region IP to Country

        private readonly object m_objIpToCountryLocker = new object();

        public string GetCountryCode(string strIP)
        {

            string strReturnCode = String.Empty;

            lock (this.m_objIpToCountryLocker)
            {

                string[] a_strSplitIP = strIP.Split(new char[] { ':' });

                if (a_strSplitIP.Length >= 1)
                {
                    if (this.OptionsSettings.UseGeoIpFileOnly == true)
                    {
                        strReturnCode = this.m_clIpToCountry.lookupCountryCodeGeoIpFile(a_strSplitIP[0]).ToLower();
                    }
                    else
                    {
                        strReturnCode = this.m_clIpToCountry.lookupCountryCode(a_strSplitIP[0]).ToLower();
                    }
                    strReturnCode = (String.Compare(strReturnCode, "--", true) == 0) ? "unknown" : strReturnCode;
                }
            }

            return strReturnCode;
        }

        public string GetCountryName(string strIP)
        {
            string strReturnName = String.Empty;

            lock (this.m_objIpToCountryLocker)
            {

                string[] a_strSplitIP = strIP.Split(new char[] { ':' });

                if (a_strSplitIP.Length >= 1)
                {
                    if (this.OptionsSettings.UseGeoIpFileOnly == true)
                    {
                        strReturnName = this.m_clIpToCountry.lookupCountryNameGeoIpFile(a_strSplitIP[0]);
                    }
                    else
                    {
                        strReturnName = this.m_clIpToCountry.lookupCountryName(a_strSplitIP[0]);
                    }
                }
            }

            return strReturnName;
        }

            return strReturnName;
        }

        #endregion

        #region Accounts

        public void SaveAccountsConfig()
        {
            if (this.LoadingAccountsFile == false && this.AccountsList != null)
            {
                FileStream stmProconConfigFile = null;

                try
                {

                    if (Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs")) == false)
                    {
                        Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs"));
                    }

                    stmProconConfigFile = new FileStream(Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs"), "accounts.cfg"), FileMode.Create);

                    if (stmProconConfigFile != null)
                    {
                        StreamWriter stwConfig = new StreamWriter(stmProconConfigFile, Encoding.UTF8);

                        stwConfig.WriteLine("/////////////////////////////////////////////");
                        stwConfig.WriteLine("// This config will be overwritten by procon.");
                        stwConfig.WriteLine("/////////////////////////////////////////////");

                        foreach (Account accAccount in this.AccountsList)
                        {
                            stwConfig.WriteLine("procon.public.accounts.create \"{0}\" \"{1}\"", accAccount.Name, accAccount.Password);
                        }

                        stwConfig.Flush();
                        stwConfig.Close();
                    }
                }
                catch (Exception e)
                {
                    FrostbiteConnection.LogError("SaveAccountsConfig", String.Empty, e);
                }
                finally
                {
                    if (stmProconConfigFile != null)
                    {
                        stmProconConfigFile.Close();
                    }
                }
            }
        }

        private void AccountsList_AccountRemoved(Account item)
        {
            item.AccountPasswordChanged -= new Account.AccountPasswordChangedHandler(accAccount_AccountPasswordChanged);

            this.SaveAccountsConfig();
        }

        private void AccountsList_AccountAdded(Account item)
        {
            item.AccountPasswordChanged += new Account.AccountPasswordChangedHandler(accAccount_AccountPasswordChanged);

            this.SaveAccountsConfig();
        }

        private void accAccount_AccountPasswordChanged(Account item)
        {
            this.SaveAccountsConfig();
        }

        #endregion

        #region Reconnection and Version Timer

        private int m_iVersionTicks = 0;
        private int m_iUsageDataTicks = 0;
        private bool m_blInitialVersionCheck = false;
        private bool m_blInitialUsageDataSent = false;
        private DateTime m_dtDayCheck = DateTime.Now;

        private XmlNode CreateNode(XmlDocument document, string key, string value)
        {
            XmlNode node = document.CreateElement(key);
            node.InnerText = value;

            return node;
        }

        private static Regex version_regex = new Regex(@"(?<major>\d+)(\.(?<minor>\d+)(\.(?<build>\d+)(\.(?<revision>\d+))?)?)?", RegexOptions.Compiled);

        private Version HighestNetFrameworkVersion()
        {
            Version highest_version = new Version();

            try
            {
                string str_monoVersion;

                // check for mono
                Type monoType = Type.GetType("Mono.Runtime");
                if (monoType != null)
                {
                    BindingFlags methodFlags = BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.ExactBinding;

                    MethodInfo monoDisplayName = monoType.GetMethod("GetDisplayName", methodFlags, null, Type.EmptyTypes, null);
                    if (monoDisplayName != null)
                    {
                        str_monoVersion = (string)monoDisplayName.Invoke(null, null);
                        string[] parts = str_monoVersion.Split('.');
                        int major = Int32.Parse(parts[0]);
                        int minor = Int32.Parse(parts[1]);
                        int revision = Int32.Parse(parts[2].Substring(0, parts[2].IndexOf(' ')));
                        Version MonoVersion = new Version(major, minor, revision);

                        highest_version = MonoVersion;
                    }
                }
                else
                {
                    // normal .Net check
                    RegistryKey installed_versions = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP");

                    string[] version_keys = installed_versions.GetSubKeyNames();

                    foreach (string version_key in version_keys)
                    {
                        Match version_match = version_regex.Match(version_key);

                        if (version_match.Success == true)
                        {
                            int service_pack = Convert.ToInt32(installed_versions.OpenSubKey(version_key).GetValue("SP", 0));

                            Version version = new Version(
                                version_match.Groups["major"].Value.Length > 0 ? int.Parse(version_match.Groups["major"].Value) : 0,
                                version_match.Groups["minor"].Value.Length > 0 ? int.Parse(version_match.Groups["minor"].Value) : 0,
                                version_match.Groups["build"].Value.Length > 0 ? int.Parse(version_match.Groups["build"].Value) : 0,
                                service_pack
                            );

                            if (version > highest_version)
                            {
                                highest_version = version;
                            }
                        }
                    }
                } // end of mono check
            }
            catch (System.ArgumentOutOfRangeException e)
            {
                // Console.WriteLine(e.Message);
            }

            return highest_version;
        }

        private string GetFrameworkName()
        {

            string FrameworkName;

            // check for mono
            Type monoType = Type.GetType("Mono.Runtime");
            if (monoType != null)
            {
                FrameworkName = "Mono.Runtime";
            }
            else
            {
                FrameworkName = ".NET";
            }

            return FrameworkName;
        }

        private void SendUsageData()
        {

            XmlDocument document = new XmlDocument();
            XmlNode usage = document.CreateElement("usage");

            XmlNode general = document.CreateElement("general");
            general.AppendChild(this.CreateNode(document, "version", Assembly.GetExecutingAssembly().GetName().Version.ToString()));
            general.AppendChild(this.CreateNode(document, "language", this.CurrentLanguage.FileName));
            general.AppendChild(this.CreateNode(document, "accounts", this.AccountsList.Count.ToString()));
            general.AppendChild(this.CreateNode(document, "uptime", (DateTime.Now - Process.GetCurrentProcess().StartTime).TotalSeconds.ToString()));
            usage.AppendChild(general);

            XmlNode environment = document.CreateElement("environment");
            environment.AppendChild(this.CreateNode(document, "platform", "desktop"));
            environment.AppendChild(this.CreateNode(document, "framework_name", this.GetFrameworkName()));
            environment.AppendChild(this.CreateNode(document, "max_framework_version", this.HighestNetFrameworkVersion().ToString()));
            usage.AppendChild(environment);

            XmlNode connections = document.CreateElement("connections");
            foreach (PRoConClient client in this.Connections)
            {
                XmlNode connection = document.CreateElement("connection");
                connection.AppendChild(this.CreateNode(document, "ip", client.HostName));
                connection.AppendChild(this.CreateNode(document, "port", client.Port.ToString()));
                connection.AppendChild(this.CreateNode(document, "game", client.GameType));
                connection.AppendChild(this.CreateNode(document, "servername", ""));
                connection.AppendChild(this.CreateNode(document, "is_layer_connection", client.IsPRoConConnection.ToString()));

                XmlNode layer = document.CreateElement("layer");
                layer.AppendChild(this.CreateNode(document, "port", client.Layer.ListeningPort.ToString()));
                layer.AppendChild(this.CreateNode(document, "is_enabled", client.Layer.IsOnline.ToString()));
                connection.AppendChild(layer);

                XmlNode plugins = document.CreateElement("plugins");

                if (client.PluginsManager != null)
                {
                    foreach (string className in new List<string>(client.PluginsManager.Plugins.LoadedClassNames))
                    {
                        XmlNode plugin = document.CreateElement("plugin");

                        PRoCon.Core.Plugin.PluginDetails details = client.PluginsManager.GetPluginDetails(className);

                        plugin.AppendChild(this.CreateNode(document, "uid", details.ClassName));
                        plugin.AppendChild(this.CreateNode(document, "name", details.Name));
                        plugin.AppendChild(this.CreateNode(document, "version", details.Version));
                        plugin.AppendChild(this.CreateNode(document, "is_enabled", client.PluginsManager.Plugins.EnabledClassNames.Contains(className).ToString()));

                        plugins.AppendChild(plugin);
                    }
                }

                connection.AppendChild(plugins);

                connections.AppendChild(connection);
            }

            usage.AppendChild(connections);

            document.AppendChild(usage);

            /*
             * Output should show the user everything regarding their procon instance that has been
             * sent to myrcon.com
             */
            document.Save("usage.xml");

            // Now append the license key for hosts and such so it's not saved to usage.xml
            // and exposed to the user.
            general.AppendChild(this.CreateNode(document, "licensekey", this.LicenseKey));

            try
            {
                // Create a request using a URL that can receive a post. 
                WebRequest request = WebRequest.Create("https://api.myrcon.net/procon/usage");
                request.Method = "POST";

                string postData = document.OuterXml;
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);

                request.ContentType = "text/xml";//"application/x-www-form-urlencoded";
                request.ContentLength = byteArray.Length;

                Stream dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();

                // Get the response.
                WebResponse response = request.GetResponse();
                Console.WriteLine(((HttpWebResponse)response).StatusDescription);
                dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();

                reader.Close();
                dataStream.Close();
                response.Close();
            }
            catch (Exception)
            {
                // Say nada
            }
        }

        private void ReconnectVersionChecker()
        {
            // Send a report naow, next in 30 mins.
            if (this.m_blInitialUsageDataSent == false && this.OptionsSettings.AllowAnonymousUsageData == true)
            {
                this.SendUsageData();

                this.m_blInitialUsageDataSent = true;
            }

            if (this.m_blInitialVersionCheck == false && this.OptionsSettings.AutoCheckDownloadUpdates == true)
            {
                this.AutoUpdater.CheckVersion();

                this.m_blInitialVersionCheck = true;
            }

            // Loop through each connection
            foreach (PRoConClient prcClient in this.Connections)
            {

                // If an error occurs
                if (prcClient.State == ConnectionState.Error
                || (prcClient.State != ConnectionState.Connected && prcClient.AutomaticallyConnect == true)
                || (this.ConsoleMode == true && prcClient.State != ConnectionState.Connected))
                {
                    prcClient.Connect();
                }

                prcClient.Poke();
            }

            // If it's ticked over to a new day..
            if (this.m_dtDayCheck.Day != DateTime.Now.Day)
            {

                foreach (PRoConClient prcClient in this.Connections)
                {
                    if (prcClient.ChatConsole != null)
                    {
                        prcClient.ChatConsole.Logging = false;
                        prcClient.ChatConsole.Logging = this.OptionsSettings.ChatLogging;
                    }

                    if (prcClient.EventsLogging != null)
                    {
                        prcClient.EventsLogging.Logging = false;
                        prcClient.EventsLogging.Logging = this.OptionsSettings.EventsLogging;
                    }

                    if (prcClient.Console != null)
                    {
                        prcClient.Console.Logging = false;
                        prcClient.Console.Logging = this.OptionsSettings.ConsoleLogging;
                    }

                    if (prcClient.PluginConsole != null)
                    {
                        prcClient.PluginConsole.Logging = false;
                        prcClient.PluginConsole.Logging = this.OptionsSettings.PluginLogging;
                    }
                }
            }

            this.m_dtDayCheck = DateTime.Now;

            // If it's been 3 hours (this ticks every 20 seconds) and we're checking for updates..

            if (this.m_iVersionTicks >= 540 && this.OptionsSettings.AutoCheckDownloadUpdates == true)
            {

                this.AutoUpdater.CheckVersion();

                this.m_iVersionTicks = 0;
            }

            // Still sends usage data every 30 minutes
            if (this.m_iUsageDataTicks >= 90 && this.OptionsSettings.AllowAnonymousUsageData == true)
            {

                this.SendUsageData();

                this.m_iUsageDataTicks = 0;
            }

            this.m_iUsageDataTicks++;
            this.m_iVersionTicks++;
        }

        #endregion

        public void Shutdown()
        {

            this.Checker.Dispose();

            this.SaveAccountsConfig();
            this.SaveMainConfig();

            this.AutoUpdater.Shutdown();

            foreach (PRoConClient pcClient in this.Connections)
            {
                pcClient.StopSound(default(PRoConClient.SPlaySound));
                pcClient.ForceDisconnect();
                pcClient.Destroy();
            }

            if (this.HttpWebServer != null)
            {
                this.HttpWebServer.Shutdown();
                this.HttpWebServer = null;
            }
        }
    }
}
