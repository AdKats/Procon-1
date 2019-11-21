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

namespace PRoCon.Controls.ServerSettings.MOHW
{
    using Core.Remote;
    public partial class uscServerSettingsConfigGeneratorMOHW : uscServerSettingsConfigGenerator
    {
        public uscServerSettingsConfigGeneratorMOHW()
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
                this.Client.Game.ThirdPersonVehicleCameras += new FrostbiteClient.IsEnabledHandler(Client_ThirdPersonVehicleCameras);
                // deprecated R-5 this.Client.Game.AllUnlocksUnlocked += new FrostbiteClient.IsEnabledHandler(Game_AllUnlocksUnlocked);
                this.Client.Game.TeamBalance += new FrostbiteClient.IsEnabledHandler(Client_TeamBalance);
                this.Client.Game.BuddyOutline += new FrostbiteClient.IsEnabledHandler(Game_BuddyOutline);
                this.Client.Game.BulletDamage += new FrostbiteClient.LimitHandler(Game_BulletDamage);

                this.Client.Game.HudCrosshair += new FrostbiteClient.IsEnabledHandler(Game_HudCrosshair);
                this.Client.Game.HudEnemyTag += new FrostbiteClient.IsEnabledHandler(Game_HudEnemyTag);
                this.Client.Game.HudExplosiveIcons += new FrostbiteClient.IsEnabledHandler(Game_HudExplosiveIcons);
                this.Client.Game.HudGameMode += new FrostbiteClient.IsEnabledHandler(Game_HudGameMode);
                this.Client.Game.HudHealthAmmo += new FrostbiteClient.IsEnabledHandler(Game_HudHealthAmmo);
                this.Client.Game.HudMinimap += new FrostbiteClient.IsEnabledHandler(Game_HudMinimap);

                this.Client.Game.HudObiturary += new FrostbiteClient.IsEnabledHandler(Game_HudObiturary);
                this.Client.Game.HudPointsTracker += new FrostbiteClient.IsEnabledHandler(Game_HudPointsTracker);
                this.Client.Game.HudUnlocks += new FrostbiteClient.IsEnabledHandler(Game_HudUnlocks);

                this.Client.Game.IdleBanRounds += new FrostbiteClient.LimitHandler(Game_IdleBanRounds);
                this.Client.Game.IdleTimeout += new FrostbiteClient.LimitHandler(Client_IdleTimeout);
                this.Client.Game.KillCam += new FrostbiteClient.IsEnabledHandler(Client_KillCam);

                this.Client.Game.PlayerManDownTime += new FrostbiteClient.LimitHandler(Game_PlayerManDownTime);
                this.Client.Game.PlayerRespawnTime += new FrostbiteClient.LimitHandler(Game_PlayerRespawnTime);
                this.Client.Game.Playlist += new FrostbiteClient.PlaylistSetHandler(Game_Playlist);
                this.Client.Game.RegenerateHealth += new FrostbiteClient.IsEnabledHandler(Game_RegenerateHealth);
                this.Client.Game.RoundRestartPlayerCount += new FrostbiteClient.LimitHandler(Game_RoundRestartPlayerCount);
                this.Client.Game.RoundStartPlayerCount += new FrostbiteClient.LimitHandler(Game_RoundStartPlayerCount);

                this.Client.Game.SoldierHealth += new FrostbiteClient.LimitHandler(Game_SoldierHealth);
                this.Client.Game.GameModeCounter += new FrostbiteClient.LimitHandler(Game_GameModeCounter);
                this.Client.Game.ServerMessage += new FrostbiteClient.ServerMessageHandler(Game_ServerMessage);
            });
        }

        protected override void Game_Login(FrostbiteClient sender)
        {
            //base.Game_Login(sender);

            this.AppendPunkbusterActivation();
        }

        private void AppendPunkbusterActivation()
        {
            this.AppendSetting("punkBuster.activate");
        }

        private void Game_Playlist(FrostbiteClient sender, string playlist)
        {
            this.AppendSetting("vars.playlist", playlist);
        }

        void Game_PlayerRespawnTime(FrostbiteClient sender, int limit)
        {
            this.AppendSetting("vars.playerRespawnTime", limit.ToString());
        }

        void Game_RoundRestartPlayerCount(FrostbiteClient sender, int limit)
        {
            this.AppendSetting("vars.roundRestartPlayerCount", limit.ToString());
        }

        void Game_RoundStartPlayerCount(FrostbiteClient sender, int limit)
        {
            this.AppendSetting("vars.roundStartPlayerCount", limit.ToString());
        }

        void Game_PlayerManDownTime(FrostbiteClient sender, int limit)
        {
            this.AppendSetting("vars.playerManDownTime", limit.ToString());
        }

        void Game_SoldierHealth(FrostbiteClient sender, int limit)
        {
            this.AppendSetting("vars.soldierHealth", limit.ToString());
        }

        void Game_RegenerateHealth(FrostbiteClient sender, bool isEnabled)
        {
            this.AppendSetting("vars.regenerateHealth", isEnabled.ToString());
        }

        void Client_ThirdPersonVehicleCameras(FrostbiteClient sender, bool isEnabled)
        {
            this.AppendSetting("vars.3pCam", isEnabled.ToString());
        }
        /* deprecated R-5
        void Game_AllUnlocksUnlocked(FrostbiteClient sender, bool isEnabled) {
            this.AppendSetting("vars.allUnlocksUnlocked", isEnabled.ToString());
        }
        */
        void Game_BuddyOutline(FrostbiteClient sender, bool isEnabled)
        {
            this.AppendSetting("vars.buddyOutline", isEnabled.ToString());
        }

        void Game_BulletDamage(FrostbiteClient sender, int limit)
        {
            this.AppendSetting("vars.bulletDamage", limit.ToString());
        }

        void Game_HudEnemyTag(FrostbiteClient sender, bool isEnabled)
        {
            this.AppendSetting("vars.hudEnemyTag", isEnabled.ToString());
        }

        void Game_HudExplosiveIcons(FrostbiteClient sender, bool isEnabled)
        {
            this.AppendSetting("vars.hudExplosiveIcons", isEnabled.ToString());
        }

        void Game_HudGameMode(FrostbiteClient sender, bool isEnabled)
        {
            this.AppendSetting("vars.hudGameMode", isEnabled.ToString());
        }

        void Game_HudCrosshair(FrostbiteClient sender, bool isEnabled)
        {
            this.AppendSetting("vars.hudCrosshair", isEnabled.ToString());
        }

        void Game_HudHealthAmmo(FrostbiteClient sender, bool isEnabled)
        {
            this.AppendSetting("vars.hudHealthAmmo", isEnabled.ToString());
        }

        void Game_HudMinimap(FrostbiteClient sender, bool isEnabled)
        {
            this.AppendSetting("vars.hudMinimap", isEnabled.ToString());
        }

        void Game_HudObiturary(FrostbiteClient sender, bool isEnabled)
        {
            this.AppendSetting("vars.hudObiturary", isEnabled.ToString());
        }

        void Game_HudPointsTracker(FrostbiteClient sender, bool isEnabled)
        {
            this.AppendSetting("vars.hudPointsTracker", isEnabled.ToString());
        }

        void Game_HudUnlocks(FrostbiteClient sender, bool isEnabled)
        {
            this.AppendSetting("vars.hudUnlocks", isEnabled.ToString());
        }

        void Client_RankLimit(FrostbiteClient sender, int limit)
        {
            this.AppendSetting("vars.rankLimit", limit.ToString());
        }

        void Client_TeamBalance(FrostbiteClient sender, bool isEnabled)
        {
            this.AppendSetting("vars.autoBalance", isEnabled.ToString());
        }

        void Client_KillCam(FrostbiteClient sender, bool isEnabled)
        {
            this.AppendSetting("vars.killCam", isEnabled.ToString());
        }


        void Game_GameModeCounter(FrostbiteClient sender, int limit)
        {
            this.AppendSetting("vars.gameModeCounter", limit.ToString());
        }

        protected override void Client_PlayerLimit(FrostbiteClient sender, int limit)
        {
            this.AppendSetting("vars.maxPlayers", limit.ToString());
        }

        protected override void Client_IdleTimeout(FrostbiteClient sender, int limit)
        {
            this.AppendSetting("vars.idleTimeout", limit.ToString());
        }

        void Game_IdleBanRounds(FrostbiteClient sender, int limit)
        {
            this.AppendSetting("vars.idleBanRounds", limit.ToString());
        }

        void Game_ServerMessage(FrostbiteClient sender, string message)
        {
            this.AppendSetting("vars.serverMessage", message);
        }



    }
}
