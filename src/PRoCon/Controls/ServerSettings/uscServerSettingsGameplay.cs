using System;
using System.Windows.Forms;

namespace PRoCon.Controls.ServerSettings
{
    using Core;
    using Core.Remote;
    public partial class uscServerSettingsGameplay : uscServerSettings
    {
        public uscServerSettingsGameplay()
        {
            InitializeComponent();

            this.AsyncSettingControls.Add("vars.hardcore", new AsyncStyleSetting(this.picSettingsHardcore, this.chkSettingsHardcore, new Control[] { this.chkSettingsHardcore }, true));
            this.AsyncSettingControls.Add("vars.friendlyfire", new AsyncStyleSetting(this.picSettingsFriendlyFire, this.chkSettingsFriendlyFire, new Control[] { this.chkSettingsFriendlyFire }, true));
        }

        public override void SetLocalization(CLocalization clocLanguage)
        {
            base.SetLocalization(clocLanguage);

            this.chkSettingsHardcore.Text = this.Language.GetLocalized("uscServerSettingsPanel.chkSettingsHardcore");
            this.chkSettingsFriendlyFire.Text = this.Language.GetLocalized("uscServerSettingsPanel.chkSettingsFriendlyFire");

            this.DisplayName = this.Language.GetLocalized("uscServerSettingsPanel.lblSettingsGameplay");
        }

        public override void SetConnection(Core.Remote.PRoConClient prcClient)
        {
            base.SetConnection(prcClient);

            if (this.Client != null)
            {
                if (this.Client.Game != null)
                {
                    this.m_prcClient_GameTypeDiscovered(prcClient);
                }
                else
                {
                    this.Client.GameTypeDiscovered += new PRoConClient.EmptyParamterHandler(m_prcClient_GameTypeDiscovered);
                }
            }
        }

        private void m_prcClient_GameTypeDiscovered(PRoConClient sender)
        {
            this.InvokeIfRequired(() =>
            {
                this.Client.Game.Hardcore += new FrostbiteClient.IsEnabledHandler(m_prcClient_Hardcore);
                this.Client.Game.FriendlyFire += new FrostbiteClient.IsEnabledHandler(m_prcClient_FriendlyFire);
            });
        }

        #region Hardcore

        private void m_prcClient_Hardcore(FrostbiteClient sender, bool isEnabled)
        {
            this.OnSettingResponse("vars.hardcore", isEnabled, true);
        }

        private void chkSettingsHardcore_CheckedChanged(object sender, EventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                if (this.IgnoreEvents == false && this.AsyncSettingControls["vars.hardcore"].IgnoreEvent == false)
                {
                    this.WaitForSettingResponse("vars.hardcore", !this.chkSettingsHardcore.Checked);

                    this.Client.Game.SendSetVarsHardCorePacket(this.chkSettingsHardcore.Checked);
                    //this.SendCommand("vars.hardCore", Packet.bltos());
                }
            }
        }

        #endregion

        #region Friendly Fire

        private void m_prcClient_FriendlyFire(FrostbiteClient sender, bool isEnabled)
        {
            this.OnSettingResponse("vars.friendlyfire", isEnabled, true);
        }

        private void chkSettingsFriendlyFire_CheckedChanged(object sender, EventArgs e)
        {
            if (this.Client != null && this.Client.Game != null)
            {
                if (this.IgnoreEvents == false && this.AsyncSettingControls["vars.friendlyfire"].IgnoreEvent == false)
                {
                    this.WaitForSettingResponse("vars.friendlyfire", !this.chkSettingsFriendlyFire.Checked);

                    this.Client.Game.SendSetVarsFriendlyFirePacket(this.chkSettingsFriendlyFire.Checked);

                    //this.SendCommand("vars.friendlyFire", Packet.bltos(this.chkSettingsFriendlyFire.Checked));
                }
            }
        }

        #endregion

    }
}
