namespace PRoCon.Controls.ServerSettings.MOH
{
    using Core.Remote;
    public partial class uscServerSettingsConfigGeneratorMoH : uscServerSettingsConfigGenerator
    {
        public uscServerSettingsConfigGeneratorMoH()
            : base()
        {
            InitializeComponent();
        }


        public override void SetConnection(Core.Remote.PRoConClient prcClient)
        {
            base.SetConnection(prcClient);

            if (this.Client != null)
            {
                if (this.Client.Game != null)
                {
                    this.Client_GameTypeDiscovered(prcClient);
                }
                else
                {
                    this.Client.GameTypeDiscovered += new PRoConClient.EmptyParamterHandler(Client_GameTypeDiscovered);
                }
            }
        }

        private void Client_GameTypeDiscovered(PRoConClient sender)
        {
            this.InvokeIfRequired(() =>
            {
                this.Client.Game.ClanTeams += new FrostbiteClient.IsEnabledHandler(Game_ClanTeams);
                this.Client.Game.NoCrosshairs += new FrostbiteClient.IsEnabledHandler(Game_NoCrosshairs);
                this.Client.Game.RealisticHealth += new FrostbiteClient.IsEnabledHandler(Game_RealisticHealth);
                this.Client.Game.NoUnlocks += new FrostbiteClient.IsEnabledHandler(Game_NoUnlocks);
                this.Client.Game.NoAmmoPickups += new FrostbiteClient.IsEnabledHandler(Game_NoAmmoPickups);

                this.Client.Game.TdmScoreCounterMaxScore += new FrostbiteClient.LimitHandler(Game_TdmScoreCounterMaxScore);
                this.Client.Game.SkillLimit += new FrostbiteClient.UpperLowerLimitHandler(Game_SkillLimit);
                this.Client.Game.RoundStartTimer += new FrostbiteClient.IsEnabledHandler(Game_RoundStartTimer);
                this.Client.Game.RoundStartTimerDelay += new FrostbiteClient.LimitHandler(Game_RoundStartTimerDelay);
                this.Client.Game.RoundStartTimerPlayerLimit += new FrostbiteClient.LimitHandler(Game_RoundStartTimerPlayerLimit);
                this.Client.Game.PreRoundLimit += new FrostbiteClient.UpperLowerLimitHandler(Game_PreRoundLimit);
            });
        }

        private void Game_PreRoundLimit(FrostbiteClient sender, int upperLimit, int lowerLimit)
        {
            this.AppendSetting("vars.preRoundLimit", upperLimit.ToString(), lowerLimit.ToString());
        }

        private void Game_RoundStartTimerPlayerLimit(FrostbiteClient sender, int limit)
        {
            this.AppendSetting("vars.roundStartTimerPlayersLimit", limit.ToString());
        }

        private void Game_RoundStartTimerDelay(FrostbiteClient sender, int limit)
        {
            this.AppendSetting("vars.roundStartTimerDelay", limit.ToString());
        }

        private void Game_RoundStartTimer(FrostbiteClient sender, bool isEnabled)
        {
            this.AppendSetting("admin.roundStartTimerEnabled", isEnabled.ToString());
        }

        private void Game_SkillLimit(FrostbiteClient sender, int upperLimit, int lowerLimit)
        {
            this.AppendSetting("vars.skillLimit", upperLimit.ToString(), lowerLimit.ToString());
        }

        private void Game_TdmScoreCounterMaxScore(FrostbiteClient sender, int limit)
        {
            this.AppendSetting("vars.tdmScoreCounterMaxScore", limit.ToString());
        }

        private void Game_ClanTeams(FrostbiteClient sender, bool isEnabled)
        {
            this.AppendSetting("vars.clanTeams", isEnabled.ToString());
        }

        private void Game_NoCrosshairs(FrostbiteClient sender, bool isEnabled)
        {
            this.AppendSetting("vars.noCrosshairs", isEnabled.ToString());
        }

        private void Game_RealisticHealth(FrostbiteClient sender, bool isEnabled)
        {
            this.AppendSetting("vars.realisticHealth", isEnabled.ToString());
        }

        private void Game_NoUnlocks(FrostbiteClient sender, bool isEnabled)
        {
            this.AppendSetting("vars.noUnlocks", isEnabled.ToString());
        }

        private void Game_NoAmmoPickups(FrostbiteClient sender, bool isEnabled)
        {
            this.AppendSetting("vars.noAmmoPickups", isEnabled.ToString());
        }

    }
}
