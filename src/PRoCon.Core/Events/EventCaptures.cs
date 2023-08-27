using System;
using System.Collections.Generic;

namespace PRoCon.Core.Events
{
    using Core.Logging;
    using Core.Players;
    using Core.Plugin;
    using Core.Remote;
    public class EventCaptures : Loggable
    {
        //private NotificationList<CapturableEvents> m_lstCapturedEvents;

        private PRoConClient m_prcClient;

        public delegate void LoggedEventHandler(CapturedEvent ceCapture);
        public event LoggedEventHandler LoggedEvent;

        public delegate void OptionsVisibleChangeHandler(bool isHidden);
        public event OptionsVisibleChangeHandler OptionsVisibleChange;

        public delegate void MaximumDisplayedEventsChangeHandler(int maximumDisplayedEvents);
        public event MaximumDisplayedEventsChangeHandler MaximumDisplayedEventsChange;

        public delegate void ScrollingEnabledChangeHandler(bool isEnabled);
        public event ScrollingEnabledChangeHandler ScrollingEnabledChange;

        public NotificationList<CapturableEvents> CapturedEvents
        {
            get;
            private set;
        }

        public Queue<CapturedEvent> LogEntries
        {
            get;
            private set;
        }

        private bool m_isOptionsVisible;
        public bool OptionsVisible
        {
            get
            {
                return this.m_isOptionsVisible;
            }
            set
            {
                this.m_isOptionsVisible = value;

                if (this.OptionsVisibleChange != null)
                {
                    this.OptionsVisibleChange(this.m_isOptionsVisible);
                }
            }
        }

        public bool IsListModified { get; private set; }

        private int m_iMaximumDisplayedEvents;
        public int MaximumDisplayedEvents
        {
            get
            {
                return this.m_iMaximumDisplayedEvents;
            }
            set
            {
                this.m_iMaximumDisplayedEvents = value;

                if (this.MaximumDisplayedEventsChange != null)
                {
                    this.MaximumDisplayedEventsChange(this.m_iMaximumDisplayedEvents);
                }
            }
        }

        private bool m_blScrollingEnabled;

        public bool ScrollingEnabled
        {
            get { return this.m_blScrollingEnabled; }
            set
            {
                this.m_blScrollingEnabled = value;

                if (this.ScrollingEnabledChange != null)
                {
                    this.ScrollingEnabledChange(this.m_blScrollingEnabled);
                }
            }
        }

        public List<string> Settings
        {
            get
            {
                List<string> lstReturnSettings = new List<string>();
                lstReturnSettings.Add(this.OptionsVisible.ToString());
                lstReturnSettings.Add(this.MaximumDisplayedEvents.ToString());
                lstReturnSettings.Add(this.IsListModified.ToString());
                lstReturnSettings.Add(this.ScrollingEnabled.ToString());

                if (this.IsListModified == true)
                {
                    foreach (CapturableEvents ceEvent in this.CapturedEvents)
                    {
                        lstReturnSettings.Add(ceEvent.ToString());
                    }
                }

                return lstReturnSettings;
            }
            set
            {
                int iMaximumCaptures = 200;
                bool isCollapsed = false;
                bool isModified = false;
                bool scrollingEnabled = false;

                if (value.Count > 0)
                {

                    if (value.Count >= 1 && bool.TryParse(value[0], out isCollapsed) == true)
                    {
                        this.OptionsVisible = isCollapsed;
                    }

                    if (value.Count >= 2 && int.TryParse(value[1], out iMaximumCaptures) == true)
                    {
                        this.MaximumDisplayedEvents = iMaximumCaptures;
                    }

                    if (value.Count >= 3 && bool.TryParse(value[2], out isModified) == true)
                    {
                        this.IsListModified = isModified;
                    }

                    if (value.Count >= 4 && bool.TryParse(value[3], out scrollingEnabled) == true)
                    {
                        this.ScrollingEnabled = scrollingEnabled;
                    }

                    if (this.IsListModified == true)
                    {
                        this.CapturedEvents.Clear();

                        for (int i = 4; i < value.Count; i++)
                        {
                            if (Enum.IsDefined(typeof(CapturableEvents), value[i]) == true && this.m_prcClient.EventsLogging.CapturedEvents.Contains((CapturableEvents)Enum.Parse(typeof(CapturableEvents), value[i])) == false)
                            {
                                this.CapturedEvents.Add((CapturableEvents)Enum.Parse(typeof(CapturableEvents), value[i]));
                            }
                        }
                    }
                }
            }
        }

        public EventCaptures(PRoConClient prcClient)
        {
            this.CapturedEvents = new NotificationList<CapturableEvents>();

            foreach (CapturableEvents item in Enum.GetValues(typeof(CapturableEvents)))
            {
                this.CapturedEvents.Add(item);
            }

            this.CapturedEvents.ItemAdded += new NotificationList<CapturableEvents>.ItemModifiedHandler(CapturedEvents_ItemAdded);
            this.CapturedEvents.ItemRemoved += new NotificationList<CapturableEvents>.ItemModifiedHandler(CapturedEvents_ItemRemoved);

            this.LogEntries = new Queue<CapturedEvent>();

            this.m_isOptionsVisible = false;
            this.m_iMaximumDisplayedEvents = 200;

            if ((this.m_prcClient = prcClient) != null)
            {
                this.FileHostNamePort = this.m_prcClient.FileHostNamePort;
                this.LoggingStartedPrefix = "Events logging started";
                this.LoggingStoppedPrefix = "Events logging stopped";
                this.FileNameSuffix = "events";

                this.m_prcClient.ConnectAttempt += new PRoConClient.EmptyParamterHandler(m_prcClient_CommandConnectAttempt);
                this.m_prcClient.ConnectSuccess += new PRoConClient.EmptyParamterHandler(m_prcClient_CommandConnectSuccess);
                this.m_prcClient.ConnectionFailure += new PRoConClient.FailureHandler(m_prcClient_ConnectionFailure);
                this.m_prcClient.SocketException += new PRoConClient.SocketExceptionHandler(m_prcClient_SocketException);
                this.m_prcClient.ConnectionClosed += new PRoConClient.EmptyParamterHandler(m_prcClient_ConnectionClosed);

                this.m_prcClient.Login += new PRoConClient.EmptyParamterHandler(m_prcClient_CommandLogin);
                this.m_prcClient.Logout += new PRoConClient.EmptyParamterHandler(m_prcClient_CommandLogout);
                this.m_prcClient.LoginFailure += new PRoConClient.AuthenticationFailureHandler(m_prcClient_CommandLoginFailure);

                this.m_prcClient.Game.LevelStarted += new FrostbiteClient.EmptyParamterHandler(m_prcClient_LevelStarted);
                this.m_prcClient.Game.LoadingLevel += new FrostbiteClient.LoadingLevelHandler(m_prcClient_LoadingLevel);
                this.m_prcClient.Game.LevelLoaded += new FrostbiteClient.LevelLoadedHandler(m_prcClient_LevelLoaded);

                this.m_prcClient.Game.PlayerJoin += new FrostbiteClient.PlayerEventHandler(m_prcClient_PlayerJoin);
                this.m_prcClient.Game.PlayerLeft += new FrostbiteClient.PlayerLeaveHandler(m_prcClient_PlayerLeft);
                this.m_prcClient.Game.PlayerDisconnected += new FrostbiteClient.PlayerDisconnectedHandler(m_prcClient_PlayerDisconnected);
                this.m_prcClient.Game.PlayerKicked += new FrostbiteClient.PlayerKickedHandler(m_prcClient_PlayerKicked);

                this.m_prcClient.Game.PlayerKickedByAdmin += new FrostbiteClient.PlayerKickedHandler(Game_PlayerKickedByAdmin);
                this.m_prcClient.Game.PlayerKilledByAdmin += new FrostbiteClient.PlayerKilledByAdminHandler(Game_PlayerKilledByAdmin);
                this.m_prcClient.Game.PlayerMovedByAdmin += new FrostbiteClient.PlayerMovedByAdminHandler(Game_PlayerMovedByAdmin);

                this.m_prcClient.Game.PlayerSpawned += new FrostbiteClient.PlayerSpawnedHandler(m_prcClient_PlayerSpawned);
                this.m_prcClient.PlayerKilled += new PRoConClient.PlayerKilledHandler(m_prcClient_PlayerKilled);
                this.m_prcClient.PunkbusterPlayerUnbanned += new PRoConClient.PunkbusterBanHandler(m_prcClient_PlayerUnbanned);

                this.m_prcClient.Game.BanListAdd += new FrostbiteClient.BanListAddHandler(m_prcClient_BanListAdd);
                this.m_prcClient.Game.BanListRemove += new FrostbiteClient.BanListRemoveHandler(m_prcClient_BanListRemove);

                this.m_prcClient.CompilingPlugins += new PRoConClient.EmptyParamterHandler(m_prcClient_CompilingPlugins);
                this.m_prcClient.RecompilingPlugins += new PRoConClient.EmptyParamterHandler(m_prcClient_RecompilingPlugins);
                this.m_prcClient.PluginsCompiled += new PRoConClient.EmptyParamterHandler(m_prcClient_PluginsCompiled);

                this.m_prcClient.Game.PlayerChangedTeam += new FrostbiteClient.PlayerTeamChangeHandler(m_prcClient_PlayerChangedTeam);
                this.m_prcClient.Game.PlayerChangedSquad += new FrostbiteClient.PlayerTeamChangeHandler(m_prcClient_PlayerChangedSquad);
            }
        }

        private void CapturedEvents_ItemRemoved(int iIndex, CapturableEvents item)
        {
            this.IsListModified = true;
        }

        private void CapturedEvents_ItemAdded(int iIndex, CapturableEvents item)
        {
            this.IsListModified = true;
        }

        private void Game_PlayerMovedByAdmin(FrostbiteClient sender, string soldierName, int destinationTeamId, int destinationSquadId, bool forceKilled)
        {
            this.ProcessEvent(EventType.Playerlist, CapturableEvents.PlayerMovedByAdmin, soldierName);
        }

        private void Game_PlayerKilledByAdmin(FrostbiteClient sender, string soldierName)
        {
            this.ProcessEvent(EventType.Playerlist, CapturableEvents.PlayerKilledByAdmin, soldierName);
        }

        private void Game_PlayerKickedByAdmin(FrostbiteClient sender, string strSoldierName, string strReason)
        {
            this.ProcessEvent(EventType.Playerlist, CapturableEvents.PlayerKickedByAdmin, strSoldierName, strReason);
        }

        private void m_prcClient_PlayerChangedSquad(FrostbiteClient sender, string strSoldierName, int iTeamID, int iSquadID)
        {
            if (this.m_prcClient.PlayerList.Contains(strSoldierName) == true)
            {
                if (this.m_prcClient.PlayerList[strSoldierName].SquadID > 0)
                {
                    if (iSquadID > 0)
                    {
                        this.ProcessEvent(EventType.Playerlist, CapturableEvents.PlayerSwitchedSquads, strSoldierName, this.m_prcClient.Language.GetLocalized("global.Squad" + this.m_prcClient.PlayerList[strSoldierName].SquadID.ToString()), this.m_prcClient.Language.GetLocalized("global.Squad" + iSquadID.ToString()));
                    }
                    else
                    {
                        this.ProcessEvent(EventType.Playerlist, CapturableEvents.PlayerSwitchedSquads, strSoldierName, this.m_prcClient.Language.GetLocalized("global.Squad" + this.m_prcClient.PlayerList[strSoldierName].SquadID.ToString()), "None");
                    }
                }
                else
                {
                    // TO DO: Localize None
                    if (iSquadID > 0)
                    {
                        this.ProcessEvent(EventType.Playerlist, CapturableEvents.PlayerSwitchedSquads, strSoldierName, "None", this.m_prcClient.Language.GetLocalized("global.Squad" + iSquadID.ToString()));
                    }
                    // else - Changed from None to None.
                }
            }
        }

        private void m_prcClient_PlayerChangedTeam(FrostbiteClient sender, string strSoldierName, int iTeamID, int iSquadID)
        {
            if (this.m_prcClient.PlayerList.Contains(strSoldierName) == true)
            {
                this.ProcessEvent(EventType.Playerlist, CapturableEvents.PlayerSwitchedTeams, strSoldierName, this.m_prcClient.GetLocalizedTeamName(this.m_prcClient.PlayerList[strSoldierName].TeamID, this.m_prcClient.CurrentServerInfo.Map, this.m_prcClient.CurrentServerInfo.GameMode), this.m_prcClient.GetLocalizedTeamName(iTeamID, this.m_prcClient.CurrentServerInfo.Map, this.m_prcClient.CurrentServerInfo.GameMode));
            }
        }
        
        private void m_prcClient_PlayerSpawned(FrostbiteClient sender, string strSoldierName, string strKit, List<string> lstWeapons, List<string> lstSpecializations)
        {
            if (this.m_prcClient.PlayerList.Contains(strSoldierName) == true) {
                this.ProcessEvent(EventType.Playerlist, CapturableEvents.PlayerSpawned, strSoldierName);
            }
        }

        private void m_prcClient_PluginsCompiled(PRoConClient sender)
        {
            this.ProcessEvent(EventType.Plugins, CapturableEvents.PluginsCompiled);
        }

        private void m_prcClient_RecompilingPlugins(PRoConClient sender)
        {
            this.ProcessEvent(EventType.Plugins, CapturableEvents.RecompilingPlugins);

            this.m_prcClient.PluginsManager.PluginLoaded += new PluginManager.PluginEmptyParameterHandler(Plugins_PluginLoaded);
            this.m_prcClient.PluginsManager.PluginDisabled += new PluginManager.PluginEmptyParameterHandler(Plugins_PluginDisabled);
            this.m_prcClient.PluginsManager.PluginEnabled += new PluginManager.PluginEmptyParameterHandler(Plugins_PluginEnabled);
        }

        private void m_prcClient_CompilingPlugins(PRoConClient sender)
        {
            this.ProcessEvent(EventType.Plugins, CapturableEvents.CompilingPlugins);

            this.m_prcClient.PluginsManager.PluginLoaded += new PluginManager.PluginEmptyParameterHandler(Plugins_PluginLoaded);
            this.m_prcClient.PluginsManager.PluginDisabled += new PluginManager.PluginEmptyParameterHandler(Plugins_PluginDisabled);
            this.m_prcClient.PluginsManager.PluginEnabled += new PluginManager.PluginEmptyParameterHandler(Plugins_PluginEnabled);
        }

        private void Plugins_PluginEnabled(string strClassName)
        {
            this.ProcessEvent(EventType.Plugins, CapturableEvents.PluginEnabled, strClassName);
        }

        private void Plugins_PluginDisabled(string strClassName)
        {
            this.ProcessEvent(EventType.Plugins, CapturableEvents.PluginDisabled, strClassName);
        }

        private void Plugins_PluginLoaded(string strClassName)
        {
            this.ProcessEvent(EventType.Plugins, CapturableEvents.PluginLoaded, strClassName);
        }

        private void m_prcClient_BanListRemove(FrostbiteClient sender, CBanInfo cbiRemovedBan)
        {
            if (String.Compare(cbiRemovedBan.IdType, "name", true) == 0)
            {
                this.ProcessEvent(EventType.Banlist, CapturableEvents.PlayerUnbanned, cbiRemovedBan.SoldierName);
            }
            else if (String.Compare(cbiRemovedBan.IdType, "ip", true) == 0)
            {
                this.ProcessEvent(EventType.Banlist, CapturableEvents.IPUnbanned, cbiRemovedBan.IpAddress);
            }
            else if (String.Compare(cbiRemovedBan.IdType, "guid", true) == 0)
            {
                this.ProcessEvent(EventType.Banlist, CapturableEvents.GUIDUnbanned, cbiRemovedBan.Guid);
            }
        }

        private void m_prcClient_BanListAdd(FrostbiteClient sender, CBanInfo cbiAddedBan)
        {

            string target = String.Empty;

            if (String.Compare(cbiAddedBan.IdType, "name", true) == 0)
            {

                target = cbiAddedBan.SoldierName;
                if (cbiAddedBan.Reason.Length > 0)
                {
                    target += " [" + cbiAddedBan.Reason + "]";
                }

                if (cbiAddedBan.BanLength.Subset == TimeoutSubset.TimeoutSubsetType.Permanent)
                {
                    this.ProcessEvent(EventType.Banlist, CapturableEvents.PlayerPermanentBanned, target);
                }
                else if (cbiAddedBan.BanLength.Subset == TimeoutSubset.TimeoutSubsetType.Round)
                {
                    this.ProcessEvent(EventType.Banlist, CapturableEvents.PlayerRoundBanned, target);
                }
                else if (cbiAddedBan.BanLength.Subset == TimeoutSubset.TimeoutSubsetType.Seconds)
                {
                    this.ProcessEvent(EventType.Banlist, CapturableEvents.PlayerTimedBanned, target, (cbiAddedBan.BanLength.Timeout / 60).ToString());
                }
            }
            else if (String.Compare(cbiAddedBan.IdType, "ip", true) == 0)
            {

                target = cbiAddedBan.IpAddress;
                if (cbiAddedBan.Reason.Length > 0)
                {
                    target += " [" + cbiAddedBan.Reason + "]";
                }

                if (cbiAddedBan.BanLength.Subset == TimeoutSubset.TimeoutSubsetType.Permanent)
                {
                    this.ProcessEvent(EventType.Banlist, CapturableEvents.IPPermanentBanned, target);
                }
                else if (cbiAddedBan.BanLength.Subset == TimeoutSubset.TimeoutSubsetType.Round)
                {
                    this.ProcessEvent(EventType.Banlist, CapturableEvents.IPRoundBanned, target);
                }
                else if (cbiAddedBan.BanLength.Subset == TimeoutSubset.TimeoutSubsetType.Seconds)
                {
                    this.ProcessEvent(EventType.Banlist, CapturableEvents.IPTimedBanned, target, (cbiAddedBan.BanLength.Timeout / 60).ToString());
                }
            }
            else if (String.Compare(cbiAddedBan.IdType, "guid", true) == 0)
            {

                target = cbiAddedBan.Guid;
                if (cbiAddedBan.Reason.Length > 0)
                {
                    target += " [" + cbiAddedBan.Reason + "]";
                }

                if (cbiAddedBan.BanLength.Subset == TimeoutSubset.TimeoutSubsetType.Permanent)
                {
                    this.ProcessEvent(EventType.Banlist, CapturableEvents.GUIDPermanentBanned, target);
                }
                else if (cbiAddedBan.BanLength.Subset == TimeoutSubset.TimeoutSubsetType.Round)
                {
                    this.ProcessEvent(EventType.Banlist, CapturableEvents.GUIDRoundBanned, target);
                }
                else if (cbiAddedBan.BanLength.Subset == TimeoutSubset.TimeoutSubsetType.Seconds)
                {
                    this.ProcessEvent(EventType.Banlist, CapturableEvents.GUIDTimedBanned, target, (cbiAddedBan.BanLength.Timeout / 60).ToString());
                }
            }
        }

        private void m_prcClient_PlayerUnbanned(PRoConClient sender, CBanInfo cbiUnbannedPlayer)
        {

            if (String.Compare(cbiUnbannedPlayer.IdType, "pbguid") == 0)
            {
                this.ProcessEvent(EventType.Banlist, CapturableEvents.PlayerUnbanned, cbiUnbannedPlayer.Guid);
            }

        }

        private void m_prcClient_PlayerKilled(PRoConClient sender, Kill kKillerVictimDetails)
        {
            if (String.Compare(kKillerVictimDetails.Killer.SoldierName, kKillerVictimDetails.Victim.SoldierName) != 0)
            {

                string WeaponType = String.Empty;
                if (kKillerVictimDetails.DamageType.Length > 0 && kKillerVictimDetails.Headshot == true)
                {
                    WeaponType = String.Format("[{0} | {1}]", this.m_prcClient.Language.GetLocalized(String.Format("global.Weapons.{0}", kKillerVictimDetails.DamageType.ToLower())), this.m_prcClient.Language.GetLocalized("uscEvents.lsvEvents.PlayerKilled.HeadShot"));
                }
                else
                {
                    WeaponType = String.Format("[{0}]", this.m_prcClient.Language.GetLocalized(String.Format("global.Weapons.{0}", kKillerVictimDetails.DamageType.ToLower())));
                }

                if (this.m_prcClient.PlayerList.Contains(kKillerVictimDetails.Killer) == true && this.m_prcClient.PlayerList.Contains(kKillerVictimDetails.Victim) == true)
                {

                    if (kKillerVictimDetails.Killer.TeamID == kKillerVictimDetails.Victim.TeamID)
                    {
                        this.ProcessEvent(EventType.Playerlist, CapturableEvents.PlayerTeamKilled, kKillerVictimDetails.Killer.SoldierName, kKillerVictimDetails.Victim.SoldierName);
                    }
                    else
                    {
                        this.ProcessEvent(EventType.Playerlist, CapturableEvents.PlayerKilled, kKillerVictimDetails.Killer.SoldierName, kKillerVictimDetails.Victim.SoldierName, WeaponType);
                    }
                }
                else
                {
                    this.ProcessEvent(EventType.Playerlist, CapturableEvents.PlayerKilled, kKillerVictimDetails.Killer.SoldierName, kKillerVictimDetails.Victim.SoldierName, WeaponType);
                }
            }
            else
            {
                this.ProcessEvent(EventType.Playerlist, CapturableEvents.PlayerSuicide, kKillerVictimDetails.Killer.SoldierName);
            }
        }

        private void m_prcClient_PlayerLeft(FrostbiteClient sender, string playerName, CPlayerInfo cpiPlayer)
        {
            this.ProcessEvent(EventType.Playerlist, CapturableEvents.PlayerLeave, playerName);
        }

        private void m_prcClient_PlayerJoin(FrostbiteClient sender, string playerName)
        {
            this.ProcessEvent(EventType.Playerlist, CapturableEvents.PlayerJoin, playerName);
        }

        private void m_prcClient_PlayerDisconnected(FrostbiteClient sender, string playerName, string reason)
        {
            this.ProcessEvent(EventType.Playerlist, CapturableEvents.PlayerDisconnected, playerName, m_prcClient.Language.GetDefaultLocalized(reason, String.Format("uscChatPanel.{0}", reason)));
        }

        private void m_prcClient_CommandLoginFailure(PRoConClient sender, string strError)
        {
            this.ProcessEvent(EventType.Connection, CapturableEvents.LoginFailure);
        }

        private void m_prcClient_CommandLogout(PRoConClient sender)
        {
            this.ProcessEvent(EventType.Connection, CapturableEvents.LoggedOut);
        }

        private void m_prcClient_CommandLogin(PRoConClient sender)
        {
            this.ProcessEvent(EventType.Connection, CapturableEvents.LoggedIn);
        }

        private void m_prcClient_ConnectionClosed(PRoConClient sender)
        {
            this.ProcessEvent(EventType.Connection, CapturableEvents.Disconnected);
        }

        private void m_prcClient_SocketException(PRoConClient sender, System.Net.Sockets.SocketException se)
        {
            this.ProcessEvent(EventType.Connection, CapturableEvents.LostConnection);
        }

        private void m_prcClient_ConnectionFailure(PRoConClient sender, Exception exception)
        {
            this.ProcessEvent(EventType.Connection, CapturableEvents.LostConnection);
        }

        private void m_prcClient_CommandConnectSuccess(PRoConClient sender)
        {
            this.ProcessEvent(EventType.Connection, CapturableEvents.Connected, this.m_prcClient.HostName, this.m_prcClient.Port.ToString());
        }

        private void m_prcClient_CommandConnectAttempt(PRoConClient sender)
        {
            this.ProcessEvent(EventType.Connection, CapturableEvents.AttemptingConnection, this.m_prcClient.HostName, this.m_prcClient.Port.ToString());
        }

        private void m_prcClient_PlayerKicked(FrostbiteClient sender, string strSoldierName, string strReason)
        {
            this.ProcessEvent(EventType.Banlist, CapturableEvents.PlayerKicked, strSoldierName, strReason);
        }

        private void m_prcClient_LevelStarted(FrostbiteClient sender)
        {
            this.ProcessEvent(EventType.Map, CapturableEvents.LevelStarted);
        }

        private void m_prcClient_LoadingLevel(FrostbiteClient sender, string mapFileName, int roundsPlayed, int roundsTotal)
        {
            this.ProcessEvent(EventType.Map, CapturableEvents.LevelLoading, mapFileName);
        }

        private void m_prcClient_LevelLoaded(FrostbiteClient sender, string mapFileName, string Gamemode, int roundsPlayed, int roundsTotal)
        {
            this.ProcessEvent(EventType.Map, CapturableEvents.LevelLoaded, mapFileName, Gamemode);
        }

        public void ProcessEvent(CapturedEvent capture)
        {

            if (this.CapturedEvents.Contains(capture.Event) == true)
            {
                capture.LoggedTime = capture.LoggedTime.ToUniversalTime().AddHours(m_prcClient.Game.UtcOffset).ToLocalTime();
                if (this.Logging == true)
                {
                    this.WriteLogLine(String.Format("{0}\t{1}\t{2}\t{3}\t{4}", capture.EventType.ToString(), capture.LoggedTime.ToString("MM/dd/yyyy HH:mm:ss"), capture.InstigatingAdmin, capture.Event.ToString(), capture.EventText.Replace("{", "{{").Replace("}", "}}")));
                }

                this.LogEntries.Enqueue(capture);

                while (this.LogEntries.Count > 100)
                {
                    this.LogEntries.Dequeue();
                }

                if (this.LoggedEvent != null)
                {
                    this.LoggedEvent(capture);
                }
            }
        }

        public void ProcessEvent(EventType etType, CapturableEvents ceEvent, params string[] a_strMessageParams)
        {
            if (this.CapturedEvents.Contains(ceEvent) == true)
            {

                DateTime dtLoggedTime = DateTime.Now; // UtcNow.AddHours(m_prcClient.Game.UTCoffset).ToLocalTime();
                string strEventText = String.Empty;

                if (this.m_prcClient.Language.LocalizedExists("uscEvents.lsvEvents." + ceEvent.ToString()) == true)
                {
                    strEventText = this.m_prcClient.Language.GetLocalized("uscEvents.lsvEvents." + ceEvent.ToString(), a_strMessageParams);
                }

                CapturedEvent newCapture = new CapturedEvent(etType, ceEvent, strEventText, dtLoggedTime, this.m_prcClient.InstigatingAccountName);

                this.ProcessEvent(newCapture);
            }
        }

    }
}
