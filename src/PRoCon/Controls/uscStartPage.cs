using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Permissions;
using System.Text.RegularExpressions;
using System.Xml;

namespace PRoCon.Controls
{
    using Newtonsoft.Json;
    using PRoCon.Core;
    using PRoCon.Core.Remote;

    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    [System.Runtime.InteropServices.ComVisibleAttribute(true)]
    public partial class uscStartPage : uscPage
    {

        public delegate void ConnectionPageHandler(string hostNamePort);
        public event ConnectionPageHandler ConnectionPage;

        private readonly PRoConApplication _proconApplication;

        private CLocalization _startPageTemplates;

        private bool _isDocumentReady;
        private XmlDocument _previousDocument;
        private XmlDocument _previousPromoDocument;

        private CLocalization _language;

        public uscStartPage(PRoConApplication proconApplication)
        {
            this._isDocumentReady = false;

            this._proconApplication = proconApplication;
            this._proconApplication.Connections.ConnectionAdded += new ConnectionDictionary.ConnectionAlteredHandler(Connections_ConnectionAdded);
            this._proconApplication.Connections.ConnectionRemoved += new ConnectionDictionary.ConnectionAlteredHandler(Connections_ConnectionRemoved);

            this._proconApplication.BeginRssUpdate += new PRoConApplication.EmptyParameterHandler(m_proconApplication_BeginRssUpdate);
            this._proconApplication.RssUpdateError += new PRoConApplication.EmptyParameterHandler(m_proconApplication_RssUpdateError);
            this._proconApplication.RssUpdateSuccess += new PRoConApplication.RssHandler(m_proconApplication_RssUpdateSuccess);

            this._proconApplication.BeginPromoUpdate += new PRoConApplication.EmptyParameterHandler(m_proconApplication_BeginRssUpdate);
            this._proconApplication.PromoUpdateError += new PRoConApplication.EmptyParameterHandler(m_proconApplication_RssUpdateError);
            this._proconApplication.PromoUpdateSuccess += new PRoConApplication.RssHandler(m_proconApplication_PromoUpdateSuccess);

            InitializeComponent();
        }

        public override void SetLocalization(CLocalization clocLanguage)
        {
            base.SetLocalization(clocLanguage);

            this._language = clocLanguage;

            if (this._isDocumentReady == true && this.webBrowser1 != null && this.webBrowser1.Document != null)
            {

                // My Connections
                this.webBrowser1.Document.InvokeScript("fnSetLocalization", new object[] { "pStartPage-lblMyConnections", clocLanguage.GetDefaultLocalized("My Connections", "pStartPage-lblMyConnections") });
                this.webBrowser1.Document.InvokeScript("fnSetLocalization", new object[] { "pStartPage-lblNoConnections", clocLanguage.GetDefaultLocalized("You do not have any connections.  Get started by creating a connection to your game server or layer", "pStartPage-lblNoConnections") });
                this.webBrowser1.Document.InvokeScript("fnSetLocalization", new object[] { "pStartPage-lblCreateConnection", clocLanguage.GetDefaultLocalized("Create Connection", "pStartPage-lblCreateConnection") });
                this.webBrowser1.Document.InvokeScript("fnSetLocalization", new object[] { "pStartPage-lblCreateConnectionLegend", clocLanguage.GetDefaultLocalized("Create Connection", "pStartPage-lblCreateConnectionLegend") });

                this.webBrowser1.Document.InvokeScript("fnSetLocalization", new object[] { "pStartPage-lblConnectionsHostnameIp", clocLanguage.GetDefaultLocalized("Hostname/IP", "pStartPage-lblConnectionsHostnameIp") });
                this.webBrowser1.Document.InvokeScript("fnSetLocalization", new object[] { "pStartPage-lblConnectionsPort", clocLanguage.GetDefaultLocalized("Port", "pStartPage-lblConnectionsPort") });
                this.webBrowser1.Document.InvokeScript("fnSetLocalization", new object[] { "pStartPage-lblConnectionsUsername", clocLanguage.GetDefaultLocalized("Username", "pStartPage-lblConnectionsUsername") });
                this.webBrowser1.Document.InvokeScript("fnSetLocalization", new object[] { "pStartPage-lblConnectionsUsernameExplanation", clocLanguage.GetDefaultLocalized("You only require a username to login to a PRoCon layer", "pStartPage-lblConnectionsUsernameExplanation") });
                this.webBrowser1.Document.InvokeScript("fnSetLocalization", new object[] { "pStartPage-lblConnectionsPassword", clocLanguage.GetDefaultLocalized("Password", "pStartPage-lblConnectionsPassword") });
                this.webBrowser1.Document.InvokeScript("fnSetLocalization", new object[] { "pStartPage-lblCancelCreateConnection", clocLanguage.GetDefaultLocalized("Cancel", "pStartPage-lblCancelCreateConnection") });
                this.webBrowser1.Document.InvokeScript("fnSetLocalization", new object[] { "pStartPage-lblConnectCreateConnection", clocLanguage.GetDefaultLocalized("Connect", "pStartPage-lblConnectCreateConnection") });

                this.webBrowser1.Document.InvokeScript("fnSetLocalization", new object[] { "pStartPage-tabPhogueNetNewsFeed", clocLanguage.GetDefaultLocalized("News Feed", "pStartPage-tabPhogueNetNewsFeed") });
                this.webBrowser1.Document.InvokeScript("fnSetLocalization", new object[] { "pStartPage-tabPackages", clocLanguage.GetDefaultLocalized("Packages", "pStartPage-tabPackages") });

                /*
                this.webBrowser1.Document.InvokeScript("fnSetLocalization", new string[] { "pStartPage-lblDonate", clocLanguage.GetDefaultLocalized("Donate", "pStartPage-lblDonate") });
                this.webBrowser1.Document.InvokeScript("fnSetLocalization", new string[] { "pStartPage-lblDonationAmount", clocLanguage.GetDefaultLocalized("Donation Amount", "pStartPage-lblDonationAmount") });
                this.webBrowser1.Document.InvokeScript("fnSetLocalization", new string[] { "pStartPage-lblDonationAmount-Currency", clocLanguage.GetDefaultLocalized("Currency: USD", "pStartPage-lblDonationAmount-Currency") });
                this.webBrowser1.Document.InvokeScript("fnSetLocalization", new string[] { "pStartPage-lblRecognize", clocLanguage.GetDefaultLocalized("I want my kudos!", "pStartPage-lblRecognize") });
                this.webBrowser1.Document.InvokeScript("fnSetLocalization", new string[] { "pStartPage-chkRecognize-lblShowOnWall", clocLanguage.GetDefaultLocalized("Show on Wall", "pStartPage-chkRecognize-lblShowOnWall") });
                this.webBrowser1.Document.InvokeScript("fnSetLocalization", new string[] { "pStartPage-chkRecognize-optDoNotShow", clocLanguage.GetDefaultLocalized("Do not show any information", "pStartPage-chkRecognize-optDoNotShow") });
                this.webBrowser1.Document.InvokeScript("fnSetLocalization", new string[] { "pStartPage-chkRecognize-optAmountDetailsComments", clocLanguage.GetDefaultLocalized("Amount, Details &amp; Comments", "pStartPage-chkRecognize-optAmountDetailsComments") });
                this.webBrowser1.Document.InvokeScript("fnSetLocalization", new string[] { "pStartPage-chkRecognize-optDetailsCommentsOnly", clocLanguage.GetDefaultLocalized("User Details &amp; Comments Only", "pStartPage-chkRecognize-optDetailsCommentsOnly") });

                this.webBrowser1.Document.InvokeScript("fnSetLocalization", new string[] { "pStartPage-chkRecognize-lblName", clocLanguage.GetDefaultLocalized("Name", "pStartPage-chkRecognize-lblName") });
                this.webBrowser1.Document.InvokeScript("fnSetLocalization", new string[] { "pStartPage-chkRecognize-lblEmail", clocLanguage.GetDefaultLocalized("Email", "pStartPage-chkRecognize-lblEmail") });
                this.webBrowser1.Document.InvokeScript("fnSetLocalization", new string[] { "pStartPage-chkRecognize-lblWebsite", clocLanguage.GetDefaultLocalized("Website", "pStartPage-chkRecognize-lblWebsite") });
                this.webBrowser1.Document.InvokeScript("fnSetLocalization", new string[] { "pStartPage-chkRecognize-lblComments", clocLanguage.GetDefaultLocalized("Comments", "pStartPage-chkRecognize-lblComments") });
                this.webBrowser1.Document.InvokeScript("fnSetLocalization", new string[] { "pStartPage-chkRecognize-lblRecognizeTime", clocLanguage.GetDefaultLocalized("Your message may take up to an hour to appear on the wall.", "pStartPage-chkRecognize-lblRecognizeTime") });

                this.webBrowser1.Document.InvokeScript("fnSetLocalization", new string[] { "pStartPage-lblMonthlySummary", clocLanguage.GetDefaultLocalized("Monthly Summary", "pStartPage-lblMonthlySummary") });
                this.webBrowser1.Document.InvokeScript("fnSetLocalization", new string[] { "pStartPage-lblDonationWall", clocLanguage.GetDefaultLocalized("Donation Wall", "pStartPage-lblDonationWall") });
                */

                this.webBrowser1.Document.InvokeScript("fnSetVariableLocalization", new object[] { "m_pStartPage_lblDeleteConnection", clocLanguage.GetDefaultLocalized("Are you sure you want <br/>to delete this connection?", "m_pStartPage_lblDeleteConnection") });

                ArrayList tableHeaders = new ArrayList {
                    "",
                    clocLanguage.GetDefaultLocalized("UID", "pStartPage-tblPackages-thUid"),
                    clocLanguage.GetDefaultLocalized("Type", "pStartPage-tblPackages-thType"),
                    clocLanguage.GetDefaultLocalized("Name", "pStartPage-tblPackages-thName"),
                    clocLanguage.GetDefaultLocalized("Version", "pStartPage-tblPackages-thVersion"),
                    clocLanguage.GetDefaultLocalized("Last Update", "pStartPage-tblPackages-thLastUpdate"),
                    clocLanguage.GetDefaultLocalized("Status", "pStartPage-tblPackages-thStatus"),
                    clocLanguage.GetDefaultLocalized("Downloads", "pStartPage-tblPackages-thDownloads"),
                    "",
                    clocLanguage.GetDefaultLocalized("Discuss/Feedback", "pStartPage-tblPackages-thDiscussFeedback"),
                    clocLanguage.GetDefaultLocalized("Author", "pStartPage-tblPackages-thAuthor"),
                    "",
                    "",
                    clocLanguage.GetDefaultLocalized("Description", "pStartPage-tblPackages-thDescription"),
                    "",
                    "",
                    clocLanguage.GetDefaultLocalized("Layer Install Status", "pStartPage-tblPackages-thLayerInstallStatus"),
                    clocLanguage.GetDefaultLocalized("Install Package", "pStartPage-tblPackages-thInstallPackage")
                };

                this.webBrowser1.Document.InvokeScript("fnSetTableHeadersLocalization", new object[] { "pStartPage-tblPackages", JSON.JsonEncode(tableHeaders) });
            }
        }

        private void UpdateConnections()
        {
            this.InvokeIfRequired(() =>
            {
                ArrayList connectionsArray = new ArrayList();

                int playerCount = 0, playerSlotsTotal = 0;

                if (this._startPageTemplates != null && this._proconApplication != null)
                {
                    foreach (PRoConClient client in this._proconApplication.Connections)
                    {

                        Hashtable connectionHtml = new Hashtable();

                        string replacedTemplate = String.Empty;

                        if (client.State == ConnectionState.Connected == true && client.IsLoggedIn == true)
                        {
                            replacedTemplate = this._startPageTemplates.GetLocalized(client.CurrentServerInfo != null ? "connections.online" : "connections.online.noInfo");
                        }
                        else if (client.State == ConnectionState.Connecting || (client.State == ConnectionState.Connected && client.IsLoggedIn == false))
                        {
                            replacedTemplate = this._startPageTemplates.GetLocalized("connections.connect-attempt.noInfo");
                        }
                        else if (client.State == ConnectionState.Error)
                        {
                            replacedTemplate = this._startPageTemplates.GetLocalized(client.CurrentServerInfo != null ? "connections.error" : "connections.error.noInfo");
                        }
                        else
                        {
                            replacedTemplate = this._startPageTemplates.GetLocalized(client.CurrentServerInfo != null ? "connections.offline" : "connections.offline.noInfo");
                        }

                        replacedTemplate = replacedTemplate.Replace("%connections.online.options%", this._startPageTemplates.GetLocalized("connections.online.options"));
                        replacedTemplate = replacedTemplate.Replace("%connections.offline.options%", this._startPageTemplates.GetLocalized("connections.offline.options"));

                        if (client.Language != null)
                        {
                            replacedTemplate = replacedTemplate.Replace("%pStartPage-lblQuickConnect%", client.Language.GetDefaultLocalized("connect", "pStartPage-lblQuickConnect"));
                            replacedTemplate = replacedTemplate.Replace("%pStartPage-lblQuickDisconnect%", client.Language.GetDefaultLocalized("disconnect", "pStartPage-lblQuickDisconnect"));
                            replacedTemplate = replacedTemplate.Replace("%pStartPage-lblQuickDelete%", client.Language.GetDefaultLocalized("delete", "pStartPage-lblQuickDelete"));
                        }
                        else
                        {
                            replacedTemplate = replacedTemplate.Replace("%pStartPage-lblQuickConnect%", "connect");
                            replacedTemplate = replacedTemplate.Replace("%pStartPage-lblQuickDisconnect%", "disconnect");
                            replacedTemplate = replacedTemplate.Replace("%pStartPage-lblQuickDelete%", "delete");
                        }



                        replacedTemplate = replacedTemplate.Replace("%server_hostnameport%", client.HostNamePort);

                        if (client.CurrentServerInfo != null)
                        {
                            replacedTemplate = replacedTemplate.Replace("%players%", client.CurrentServerInfo.PlayerCount.ToString(CultureInfo.InvariantCulture));
                            replacedTemplate = replacedTemplate.Replace("%max_players%", client.CurrentServerInfo.MaxPlayerCount.ToString(CultureInfo.InvariantCulture));
                            replacedTemplate = replacedTemplate.Replace("%server_name%", client.CurrentServerInfo.ServerName);

                            if (this._proconApplication != null && this._proconApplication.CurrentLanguage != null)
                            {
                                CMap tmpMap = client.GetFriendlyMapByFilenamePlayList(client.CurrentServerInfo.Map, client.CurrentServerInfo.GameMode);
                                int iTmpCurRounds = client.CurrentServerInfo.CurrentRound;
                                if ((client.GameType == "BF3" || client.GameType == "MOHW" || client.GameType == "BF4" || client.GameType == "BFHL") && (client.CurrentServerInfo.CurrentRound != client.CurrentServerInfo.TotalRounds))
                                {
                                    iTmpCurRounds++;
                                }

                                if (tmpMap != null)
                                {
                                    replacedTemplate = replacedTemplate.Replace("%server_additonal%", this._startPageTemplates.GetLocalized("connections.online.additional", tmpMap.GameMode, tmpMap.PublicLevelName, iTmpCurRounds.ToString(CultureInfo.InvariantCulture), client.CurrentServerInfo.TotalRounds.ToString(CultureInfo.InvariantCulture)));
                                }
                            }

                            playerCount += client.CurrentServerInfo.PlayerCount;
                            playerSlotsTotal += client.CurrentServerInfo.MaxPlayerCount;
                        }

                        if (client.ConnectionServerName != String.Empty)
                        {
                            replacedTemplate = replacedTemplate.Replace("%server_name%", client.ConnectionServerName);
                        }

                        connectionHtml.Add("safehostport", Regex.Replace(client.FileHostNamePort, "[^0-9a-zA-Z]", ""));
                        connectionHtml.Add("html", replacedTemplate);

                        connectionsArray.Add(connectionHtml);
                    }

                    if (this.webBrowser1.IsDisposed == false && this.webBrowser1.Document != null)
                    {
                        if (playerSlotsTotal > 0 && this._language != null && this._isDocumentReady == true)
                        {
                            this.webBrowser1.Document.InvokeScript("fnSetLocalization", new object[] { "pStartPage-lblConnectionsSummary", this._language.GetDefaultLocalized(String.Format("{0} of {1} slots used", playerCount, playerSlotsTotal), "pStartPage-lblConnectionsSummary", playerCount, playerSlotsTotal) });
                        }

                        if (this._isDocumentReady == true)
                        {
                            // Convert to JSON the connectionsArray using Newtonsoft.Json
                            string json = JsonConvert.SerializeObject(connectionsArray);

                            this.webBrowser1.Document.InvokeScript("fnUpdateConnectionsList", new object[] { json });
                        }
                    }
                }
            });
        }

        private void Connections_ConnectionAdded(PRoConClient item)
        {
            item.ConnectionClosed += new PRoConClient.EmptyParamterHandler(item_ConnectionClosed);
            item.ConnectAttempt += new PRoConClient.EmptyParamterHandler(item_ConnectAttempt);
            item.Login += new PRoConClient.EmptyParamterHandler(item_Login);
            item.GameTypeDiscovered += new PRoConClient.EmptyParamterHandler(item_GameTypeDiscovered);

            this.UpdateConnections();
        }

        private void Connections_ConnectionRemoved(PRoConClient item)
        {
            this.InvokeIfRequired(() =>
            {
                item.ConnectionClosed -= new PRoConClient.EmptyParamterHandler(item_ConnectionClosed);
                item.Login -= new PRoConClient.EmptyParamterHandler(item_Login);
                item.GameTypeDiscovered -= new PRoConClient.EmptyParamterHandler(item_GameTypeDiscovered);

                if (item.Game != null)
                {
                    item.Game.ServerInfo -= new FrostbiteClient.ServerInfoHandler(Game_ServerInfo);
                }

                if (this._isDocumentReady == true && this.webBrowser1.Document != null)
                {
                    this.webBrowser1.Document.InvokeScript("fnRemoveConnection", new object[] { Regex.Replace(item.FileHostNamePort, "[^0-9a-zA-Z]", "") });
                }
            });
        }

        private void item_GameTypeDiscovered(PRoConClient sender)
        {
            this.InvokeIfRequired(() =>
            {
                if (sender.Game != null)
                {
                    sender.Game.ServerInfo += new FrostbiteClient.ServerInfoHandler(Game_ServerInfo);
                }
            });
        }

        private void item_ConnectAttempt(PRoConClient sender)
        {
            this.UpdateConnections();
        }

        private void item_Login(PRoConClient sender)
        {
            this.UpdateConnections();
        }

        private void item_ConnectionClosed(PRoConClient sender)
        {
            this.UpdateConnections();
        }

        private void Game_ServerInfo(FrostbiteClient sender, CServerInfo csiServerInfo)
        {
            this.UpdateConnections();
        }

        #region COM Calls

        // ReSharper disable InconsistentNaming
        public void HREF(string url)
        {
            // ReSharper restore InconsistentNaming
            if (Regex.Match(url, @"http(s)?\:\/\/.*").Success == false)
            {
                url = String.Format("http://{0}", url);
            }

            System.Diagnostics.Process.Start(url);
        }

        public void DocumentReady()
        {
            this._isDocumentReady = true;

            this.UpdateConnections();
            if (this._previousDocument != null)
            {
                this.ReplaceRssContent(this._previousDocument);

                // this.webBrowser1.Document.InvokeScript("UpdatePackageList", new object[] { this.m_proconApplication.PackageManager.RemoteToJsonString() });
            }
            if (this._previousPromoDocument != null)
            {
                this.ReplacePromoContent(this._previousPromoDocument);
            }

            this.SetLocalization(this._proconApplication.CurrentLanguage);
        }

        public void CreateConnection(string hostName, string port, string username, string password)
        {

            ushort parsedPort = 0;

            if (ushort.TryParse(port, out parsedPort) == true)
            {
                PRoConClient newConnection = this._proconApplication.AddConnection(hostName, parsedPort, username, password);

                if (newConnection != null)
                {
                    newConnection.Connect();
                }
            }
        }

        public void GoToConnectionPage(string hostNamePort)
        {
            if (this.ConnectionPage != null)
            {
                this.ConnectionPage(hostNamePort);
            }
        }

        public void AttemptConnection(string hostNamePort)
        {
            if (this._proconApplication != null && this._proconApplication.Connections != null && this._proconApplication.Connections.Contains(hostNamePort) == true)
            {
                this._proconApplication.Connections[hostNamePort].Connect();
            }
        }

        public void DisconnectConnection(string hostNamePort)
        {
            if (this._proconApplication != null && this._proconApplication.Connections != null && this._proconApplication.Connections.Contains(hostNamePort) == true)
            {
                this._proconApplication.Connections[hostNamePort].Disconnect();
                this._proconApplication.Connections[hostNamePort].AutomaticallyConnect = false;
            }
        }

        public void DeleteConnection(string hostNamePort)
        {
            if (this._proconApplication != null && this._proconApplication.Connections != null && this._proconApplication.Connections.Contains(hostNamePort) == true)
            {
                this._proconApplication.Connections[hostNamePort].Disconnect();
                this._proconApplication.Connections.Remove(hostNamePort);
            }
        }

        #endregion

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            string startPagePath = Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Media"), "UI");

            this._isDocumentReady = false;
            if (File.Exists(Path.Combine(startPagePath, "startPage.temp")) == true)
            {
                this._startPageTemplates = new CLocalization(Path.Combine(startPagePath, "startPage.temp"), "startPage.temp");
            }
            this.webBrowser1.AllowNavigation = false;
            this.webBrowser1.AllowWebBrowserDrop = false;
            this.webBrowser1.IsWebBrowserContextMenuEnabled = false;
            this.webBrowser1.ObjectForScripting = this;
            this.webBrowser1.ScriptErrorsSuppressed = true;

            if (this.webBrowser1.Document == null && File.Exists(Path.Combine(startPagePath, "startpage.html")) == true)
            {
                this.webBrowser1.Navigate(Path.Combine(startPagePath, "startpage.html"));
            }
        }

        #region RSS Feed

        private void ReplaceRssContent(XmlDocument rssDocument)
        {
            try
            {
                this._previousDocument = rssDocument;

                this.DispatchArticleFeed(rssDocument);
                //this.DispatchUserSummaryFeed(rssDocument);
                // this.DispatchDonationFeed(rssDocument);
                //this.DispatchPromotions(rssDocument);
            }
            catch (Exception)
            {
                this._previousDocument = null;
            }
        }

        private void ReplacePromoContent(XmlDocument rssDocument)
        {
            try
            {
                this._previousPromoDocument = rssDocument;
                this.DispatchPromotions(rssDocument);
            }
            catch (Exception)
            {
                this._previousPromoDocument = null;
            }
        }

        private void m_proconApplication_RssUpdateSuccess(PRoConApplication instance, XmlDocument rssDocument)
        {
            this.InvokeIfRequired(() => this.ReplaceRssContent(rssDocument));
        }

        private void m_proconApplication_PromoUpdateSuccess(PRoConApplication instance, XmlDocument rssDocument)
        {
            this.InvokeIfRequired(() => this.ReplacePromoContent(rssDocument));
        }

        private void m_proconApplication_RssUpdateError(PRoConApplication instance)
        {
            this.InvokeIfRequired(() =>
            {
                if (this._isDocumentReady == true && this.webBrowser1.Document != null)
                {
                    this.webBrowser1.Document.InvokeScript("UpdateRssMonthlySummaryFeed", new object[] { "" });
                    this.webBrowser1.Document.InvokeScript("UpdateRssDonationFeed", new object[] { "" });
                    this.webBrowser1.Document.InvokeScript("UpdateRssFeed", new object[] { "" });
                }
            });
        }

        private void m_proconApplication_BeginRssUpdate(PRoConApplication instance)
        {
            this.InvokeIfRequired(() =>
            {
                if (this._isDocumentReady == true && this.webBrowser1.Document != null)
                {
                    this.webBrowser1.Document.InvokeScript("LoadingRssFeed");
                }
            });
        }

        internal class RssArticleItem
        {

            public string Title
            {
                get;
                internal set;
            }

            public string Link
            {
                get;
                internal set;
            }

            public DateTime PublishDate
            {
                get;
                internal set;
            }

            public string Content
            {
                get;
                internal set;
            }
        }

        internal class RssDonationItem
        {

            public string Currency
            {
                get;
                internal set;
            }

            public float Amount
            {
                get;
                internal set;
            }

            public DateTime PublishDate
            {
                get;
                internal set;
            }

            public string Name
            {
                get;
                internal set;
            }

            public string Link
            {
                get;
                internal set;
            }

            public string Comment
            {
                get;
                internal set;
            }
        }

        internal class RssUserSummaryItem
        {

            public int Count
            {
                get;
                internal set;
            }
        }

        private void DispatchPromotions(XmlNode rssDocument)
        {
            try
            {
                ArrayList promotionsList = new ArrayList();

                if (rssDocument != null)
                {
                    XmlNodeList nodes = rssDocument.SelectNodes("/xml/procon/promotions/promotion");

                    if (nodes != null)
                    {
                        foreach (XmlNode node in nodes)
                        {
                            if (node != null)
                            {
                                XmlNode image = node.SelectSingleNode("image");
                                XmlNode link = node.SelectSingleNode("link");
                                XmlNode name = node.SelectSingleNode("name");

                                Hashtable promotion = new Hashtable {
                                    {"image", image != null ? image.InnerText : "" },
                                    {"link", link != null ? link.InnerText : "" },
                                    {"name", name != null ? name.InnerText : "" }
                                };
                                promotionsList.Add(promotion);
                            }
                        }
                    }
                }

                if (this._isDocumentReady == true && this.webBrowser1.Document != null)
                {
                    this.webBrowser1.Document.InvokeScript("UpdatePromotions", new object[] { JSON.JsonEncode(promotionsList) });
                }
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch (Exception) { }
            // ReSharper restore EmptyGeneralCatchClause
        }

        private void DispatchArticleFeed(XmlDocument rssDocument)
        {

            try
            {
                List<RssArticleItem> rssItems = new List<RssArticleItem>();


                if (rssDocument != null)
                {
                    XmlNodeList nodes = rssDocument.SelectNodes("rss/channel/item");

                    if (nodes != null)
                    {
                        foreach (XmlNode node in nodes)
                        {
                            if (node != null)
                            {
                                XmlNode pubDateNode = node.SelectSingleNode("pubDate");
                                XmlNode title = node.SelectSingleNode("title");
                                XmlNode link = node.SelectSingleNode("link");
                                XmlNode description = node.SelectSingleNode("description");

                                if (pubDateNode != null && title != null && link != null && description != null)
                                {
                                    DateTime pubDate = DateTime.Now.AddMonths(-1);
                                    DateTime.TryParse(pubDateNode.InnerText, out pubDate);

                                    rssItems.Add(new RssArticleItem()
                                    {
                                        Title = title.InnerText,
                                        Link = link.InnerText,
                                        Content = description.InnerText,
                                        PublishDate = pubDate
                                    });
                                }
                            }
                        }
                    }
                }

                this.UpdateRssArticleFeed(rssItems);
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch (Exception) { }
            // ReSharper restore EmptyGeneralCatchClause
        }

        private void UpdateRssArticleFeed(IEnumerable<RssArticleItem> rss)
        {

            if (this._startPageTemplates != null)
            {

                string rssHtml = String.Empty;

                foreach (RssArticleItem item in rss)
                {

                    string replacedTemplate = this._startPageTemplates.GetLocalized("newsFeed.article");

                    replacedTemplate = replacedTemplate.Replace("%article_link%", item.Link);
                    replacedTemplate = replacedTemplate.Replace("%article_title%", item.Title);
                    replacedTemplate = replacedTemplate.Replace("%article_date%", item.PublishDate.ToShortDateString());
                    replacedTemplate = replacedTemplate.Replace("%article_content%", item.Content);

                    rssHtml += replacedTemplate;

                    //rssHtml += String.Format(@"<h2><a onclick=""window.external.HREF('{0}')"">{1}</a></h2>", item.Link, item.Title);

                    //item.Content = Regex.Replace(item.Content, @"[^\[]\.\.\.($|[^\]])", " [...]", RegexOptions.IgnoreCase);
                    //item.Content = item.Content.Replace("[...]", String.Format(@"<a onclick=""window.external.HREF('{0}')"">[...]</a>", item.Link));

                    //rssHtml += String.Format("<p><b>{0}</b></p><p>{1}</p>", item.PublishDate.ToShortDateString(), item.Content);
                }

                if (this._isDocumentReady == true && this.webBrowser1.Document != null)
                {
                    this.webBrowser1.Document.InvokeScript("UpdateRssFeed", new object[] { rssHtml });
                }
            }
        }

        #endregion

    }
}
