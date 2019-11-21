namespace PRoCon.Core.Settings
{
    using Core.Remote;

    /// <summary>
    /// this class holds the general server settings of the game server
    /// 
    /// @copyright by macx
    /// </summary>
    public class ServerSettings
    {
        private PRoConClient m_prcClient;

        public string GamePassword { get; set; }
        public string AdminPassword { get; set; }

        public bool IsPunkbusterActivated { get; set; }
        public bool IsRanked { get; set; }
        public bool IsFriendlyFireActivated { get; set; }
        public bool IsTeamBalanceActivated { get; set; }
        public bool IsRankLimitActivated { get; set; }
        public int CurrentRankLimit { get; set; }
        public int CurrentPlayerLimit { get; set; }

        public bool IsHardcoreActivated { get; set; }
        public bool IsKillCamActivated { get; set; }
        public bool IsMiniMapActivated { get; set; }
        public bool IsCrossHairActivated { get; set; }
        public bool Is3DSpottingActivated { get; set; }
        public bool IsMiniMapSpottingActivated { get; set; }
        public bool IsThirdPersonVehicleCamActivated { get; set; }

        public string PublicDescription { get; set; }
        public string BannerUrl { get; set; }

        public ServerSettings()
        {

        }

        public ServerSettings(PRoConClient prcClient)
        {
            this.m_prcClient = prcClient;

            this.m_prcClient.Game.GamePassword += new FrostbiteClient.PasswordHandler(m_prcClient_GamePassword);
            this.m_prcClient.Game.AdminPassword += new FrostbiteClient.PasswordHandler(m_prcClient_AdminPassword);
            this.m_prcClient.Game.Punkbuster += new FrostbiteClient.IsEnabledHandler(m_prcClient_Punkbuster);
            this.m_prcClient.Game.Hardcore += new FrostbiteClient.IsEnabledHandler(m_prcClient_Hardcore);
            this.m_prcClient.Game.Ranked += new FrostbiteClient.IsEnabledHandler(m_prcClient_Ranked);
            this.m_prcClient.Game.RankLimit += new FrostbiteClient.LimitHandler(m_prcClient_RankLimit);
            this.m_prcClient.Game.TeamBalance += new FrostbiteClient.IsEnabledHandler(m_prcClient_TeamBalance);
            this.m_prcClient.Game.FriendlyFire += new FrostbiteClient.IsEnabledHandler(m_prcClient_FriendlyFire);
            this.m_prcClient.Game.PlayerLimit += new FrostbiteClient.LimitHandler(m_prcClient_PlayerLimit);
            this.m_prcClient.Game.MaxPlayerLimit += new FrostbiteClient.LimitHandler(m_prcClient_MaxPlayerLimit);
            this.m_prcClient.Game.CurrentPlayerLimit += new FrostbiteClient.LimitHandler(m_prcClient_CurrentPlayerLimit);
            this.m_prcClient.Game.BannerUrl += new FrostbiteClient.BannerUrlHandler(m_prcClient_BannerUrl);
            this.m_prcClient.Game.ServerDescription += new FrostbiteClient.ServerDescriptionHandler(m_prcClient_ServerDescription);
            this.m_prcClient.Game.KillCam += new FrostbiteClient.IsEnabledHandler(m_prcClient_KillCam);
            this.m_prcClient.Game.MiniMap += new FrostbiteClient.IsEnabledHandler(m_prcClient_MiniMap);
            this.m_prcClient.Game.CrossHair += new FrostbiteClient.IsEnabledHandler(m_prcClient_CrossHair);
            this.m_prcClient.Game.ThreeDSpotting += new FrostbiteClient.IsEnabledHandler(m_prcClient_ThreeDSpotting);
            this.m_prcClient.Game.MiniMapSpotting += new FrostbiteClient.IsEnabledHandler(m_prcClient_MiniMapSpotting);
            this.m_prcClient.Game.ThirdPersonVehicleCameras += new FrostbiteClient.IsEnabledHandler(m_prcClient_ThirdPersonVehicleCameras);

        }

        private void m_prcClient_ThirdPersonVehicleCameras(FrostbiteClient sender, bool isEnabled)
        {
            this.IsThirdPersonVehicleCamActivated = isEnabled;
        }

        private void m_prcClient_MiniMapSpotting(FrostbiteClient sender, bool isEnabled)
        {
            this.IsMiniMapSpottingActivated = isEnabled;
        }

        private void m_prcClient_ThreeDSpotting(FrostbiteClient sender, bool isEnabled)
        {
            this.Is3DSpottingActivated = isEnabled;
        }

        private void m_prcClient_CrossHair(FrostbiteClient sender, bool isEnabled)
        {
            this.IsCrossHairActivated = isEnabled;
        }

        private void m_prcClient_MiniMap(FrostbiteClient sender, bool isEnabled)
        {
            this.IsMiniMapActivated = isEnabled;
        }

        private void m_prcClient_KillCam(FrostbiteClient sender, bool isEnabled)
        {
            this.IsKillCamActivated = isEnabled;
        }

        private void m_prcClient_ServerDescription(FrostbiteClient sender, string serverDescription)
        {
            this.PublicDescription = serverDescription;
        }

        private void m_prcClient_BannerUrl(FrostbiteClient sender, string url)
        {
            this.BannerUrl = url;
        }

        private void m_prcClient_CurrentPlayerLimit(FrostbiteClient sender, int limit)
        {
            this.CurrentPlayerLimit = limit;
        }

        private void m_prcClient_MaxPlayerLimit(FrostbiteClient sender, int limit)
        {

        }

        private void m_prcClient_PlayerLimit(FrostbiteClient sender, int limit)
        {

        }

        private void m_prcClient_FriendlyFire(FrostbiteClient sender, bool isEnabled)
        {
            this.IsFriendlyFireActivated = isEnabled;
        }

        private void m_prcClient_TeamBalance(FrostbiteClient sender, bool isEnabled)
        {
            this.IsTeamBalanceActivated = isEnabled;
        }

        private void m_prcClient_RankLimit(FrostbiteClient sender, int limit)
        {
            if (limit == -1)
            {
                this.IsRankLimitActivated = true;
            }
            else
            {
                this.CurrentRankLimit = limit;
            }
        }

        private void m_prcClient_Ranked(FrostbiteClient sender, bool isEnabled)
        {
            this.IsRanked = isEnabled;
        }

        private void m_prcClient_Hardcore(FrostbiteClient sender, bool isEnabled)
        {
            this.IsHardcoreActivated = isEnabled;
        }

        private void m_prcClient_Punkbuster(FrostbiteClient sender, bool isEnabled)
        {
            this.IsPunkbusterActivated = isEnabled;
        }

        private void m_prcClient_AdminPassword(FrostbiteClient sender, string password)
        {
            this.AdminPassword = password;
        }

        private void m_prcClient_GamePassword(FrostbiteClient sender, string password)
        {
            this.GamePassword = password;
        }
    }
}