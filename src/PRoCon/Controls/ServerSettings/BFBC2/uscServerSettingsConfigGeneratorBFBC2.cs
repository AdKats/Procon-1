/*  Copyright 2010 Geoffrey 'Phogue' Green

    http://www.phogue.net
 
    This file is part of PRoCon Frostbite.

    PRoCon Frostbite is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    PRoCon Frostbite is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with PRoCon Frostbite.  If not, see <http://www.gnu.org/licenses/>.
 */

namespace PRoCon.Controls.ServerSettings.BFBC2
{
    using Core.Remote;
    public partial class uscServerSettingsConfigGeneratorBFBC2 : uscServerSettingsConfigGenerator
    {
        public uscServerSettingsConfigGeneratorBFBC2()
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
                this.Client.Game.RankLimit += new FrostbiteClient.LimitHandler(Client_RankLimit);
                this.Client.Game.TeamBalance += new FrostbiteClient.IsEnabledHandler(Client_TeamBalance);
                this.Client.Game.KillCam += new FrostbiteClient.IsEnabledHandler(Client_KillCam);
                this.Client.Game.MiniMap += new FrostbiteClient.IsEnabledHandler(Client_MiniMap);
                this.Client.Game.CrossHair += new FrostbiteClient.IsEnabledHandler(Client_CrossHair);
                this.Client.Game.ThreeDSpotting += new FrostbiteClient.IsEnabledHandler(Client_ThreeDSpotting);
                this.Client.Game.ThirdPersonVehicleCameras += new FrostbiteClient.IsEnabledHandler(Client_ThirdPersonVehicleCameras);
                this.Client.Game.MiniMapSpotting += new FrostbiteClient.IsEnabledHandler(Client_MiniMapSpotting);
            });
        }

        void Client_RankLimit(FrostbiteClient sender, int limit)
        {
            this.AppendSetting("vars.rankLimit", limit.ToString());
        }

        void Client_TeamBalance(FrostbiteClient sender, bool isEnabled)
        {
            this.AppendSetting("vars.teamBalance", isEnabled.ToString());
        }

        void Client_KillCam(FrostbiteClient sender, bool isEnabled)
        {
            this.AppendSetting("vars.killCam", isEnabled.ToString());
        }

        void Client_MiniMap(FrostbiteClient sender, bool isEnabled)
        {
            this.AppendSetting("vars.miniMap", isEnabled.ToString());
        }

        void Client_CrossHair(FrostbiteClient sender, bool isEnabled)
        {
            this.AppendSetting("vars.crossHair", isEnabled.ToString());
        }

        void Client_ThreeDSpotting(FrostbiteClient sender, bool isEnabled)
        {
            this.AppendSetting("vars.3dSpotting", isEnabled.ToString());
        }

        void Client_ThirdPersonVehicleCameras(FrostbiteClient sender, bool isEnabled)
        {
            this.AppendSetting("vars.thirdPersonVehicleCameras", isEnabled.ToString());
        }

        void Client_MiniMapSpotting(FrostbiteClient sender, bool isEnabled)
        {
            this.AppendSetting("vars.miniMapSpotting", isEnabled.ToString());
        }
    }
}
