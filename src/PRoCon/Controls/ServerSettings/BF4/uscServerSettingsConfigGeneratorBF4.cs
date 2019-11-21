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

namespace PRoCon.Controls.ServerSettings.BF4
{
    using Core.Remote;
    public partial class uscServerSettingsConfigGeneratorBF4 : uscServerSettingsConfigGenerator
    {
        public uscServerSettingsConfigGeneratorBF4()
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
                this.Client.Game.MaxSpectators += new FrostbiteClient.LimitHandler(Game_MaxSpectators);
                this.Client.Game.FairFight += new FrostbiteClient.IsEnabledHandler(Game_FairFight);

                this.Client.Game.TeamBalance += new FrostbiteClient.IsEnabledHandler(Client_TeamBalance);
                this.Client.Game.KillCam += new FrostbiteClient.IsEnabledHandler(Client_KillCam);
                this.Client.Game.MiniMap += new FrostbiteClient.IsEnabledHandler(Client_MiniMap);
                this.Client.Game.CrossHair += new FrostbiteClient.IsEnabledHandler(Client_CrossHair);
                this.Client.Game.ThreeDSpotting += new FrostbiteClient.IsEnabledHandler(Client_ThreeDSpotting);
                this.Client.Game.ThirdPersonVehicleCameras += new FrostbiteClient.IsEnabledHandler(Client_ThirdPersonVehicleCameras);
                this.Client.Game.MiniMapSpotting += new FrostbiteClient.IsEnabledHandler(Client_MiniMapSpotting);

                this.Client.Game.VehicleSpawnAllowed += new FrostbiteClient.IsEnabledHandler(Game_VehicleSpawnAllowed);
                this.Client.Game.VehicleSpawnDelay += new FrostbiteClient.LimitHandler(Game_VehicleSpawnDelay);
                this.Client.Game.BulletDamage += new FrostbiteClient.LimitHandler(Game_BulletDamage);
                this.Client.Game.NameTag += new FrostbiteClient.IsEnabledHandler(Game_NameTag);
                this.Client.Game.RegenerateHealth += new FrostbiteClient.IsEnabledHandler(Game_RegenerateHealth);
                this.Client.Game.OnlySquadLeaderSpawn += new FrostbiteClient.IsEnabledHandler(Game_OnlySquadLeaderSpawn);
                this.Client.Game.UnlockMode += new FrostbiteClient.UnlockModeHandler(Game_UnlockMode);
                this.Client.Game.BF4preset += new FrostbiteClient.BF4presetHandler(Game_BF4preset);
                this.Client.Game.GunMasterWeaponsPreset += new FrostbiteClient.GunMasterWeaponsPresetHandler(Game_GunMasterWeaponsPreset);
                this.Client.Game.SoldierHealth += new FrostbiteClient.LimitHandler(Game_SoldierHealth);
                this.Client.Game.Hud += new FrostbiteClient.IsEnabledHandler(Game_Hud);
                this.Client.Game.PlayerManDownTime += new FrostbiteClient.LimitHandler(Game_PlayerManDownTime);
                this.Client.Game.RoundRestartPlayerCount += new FrostbiteClient.LimitHandler(Game_RoundRestartPlayerCount);
                this.Client.Game.RoundStartPlayerCount += new FrostbiteClient.LimitHandler(Game_RoundStartPlayerCount);
                this.Client.Game.PlayerRespawnTime += new FrostbiteClient.LimitHandler(Game_PlayerRespawnTime);
                this.Client.Game.GameModeCounter += new FrostbiteClient.LimitHandler(Game_GameModeCounter);
                this.Client.Game.RoundTimeLimit += new FrostbiteClient.LimitHandler(Game_RoundTimeLimit);
                this.Client.Game.TicketBleedRate += new FrostbiteClient.LimitHandler(Game_TicketBleedRate);
                this.Client.Game.IdleTimeout += new FrostbiteClient.LimitHandler(Client_IdleTimeout);
                this.Client.Game.IdleBanRounds += new FrostbiteClient.LimitHandler(Game_IdleBanRounds);
                this.Client.Game.ServerMessage += new FrostbiteClient.ServerMessageHandler(Game_ServerMessage);

                this.Client.Game.IsHitIndicator += new FrostbiteClient.IsEnabledHandler(Game_IsHitIndicator);
                this.Client.Game.IsCommander += new FrostbiteClient.IsEnabledHandler(Game_IsCommander);
                this.Client.Game.ServerType += new FrostbiteClient.VarsStringHandler(Game_ServerType);

                this.Client.Game.AlwaysAllowSpectators += new FrostbiteClient.IsEnabledHandler(Game_AlwaysAllowSpectators);

                this.Client.Game.ReservedSlotsListAggressiveJoin += new FrostbiteClient.IsEnabledHandler(Game_ReservedSlotsListAggressiveJoin);
                this.Client.Game.RoundLockdownCountdown += new FrostbiteClient.LimitHandler(Game_RoundLockdownCountdown);
                this.Client.Game.RoundWarmupTimeout += new FrostbiteClient.LimitHandler(Game_RoundWarmupTimeout);

                this.Client.Game.TeamFactionOverride += new FrostbiteClient.TeamFactionOverrideHandler(Game_TeamFactionOverride);
                // not used in BF4 //this.Client.Game.PremiumStatus += new FrostbiteClient.IsEnabledHandler(Game_PremiumStatus);
            });
        }

        protected override void Game_Login(FrostbiteClient sender)
        {
            this.InvokeIfRequired(() =>
            {
                base.Game_Login(sender);

                this.AppendPunkbusterActivation();
            });
        }

        private void AppendPunkbusterActivation()
        {
            this.AppendSetting("punkBuster.activate");
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

        void Game_Hud(FrostbiteClient sender, bool isEnabled)
        {
            this.AppendSetting("vars.hud", isEnabled.ToString());
        }

        void Game_SoldierHealth(FrostbiteClient sender, int limit)
        {
            this.AppendSetting("vars.soldierHealth", limit.ToString());
        }

        void Game_UnlockMode(FrostbiteClient sender, string mode)
        {
            this.AppendSetting("vars.unlockMode", mode.ToLower());
        }

        void Game_BF4preset(FrostbiteClient sender, string mode, bool isLocked)
        {
            this.AppendSetting("vars.preset", mode, isLocked.ToString());
        }

        void Game_GunMasterWeaponsPreset(FrostbiteClient sender, int preset)
        {
            this.AppendSetting("vars.gunMasterWeaponsPreset", preset.ToString());
        }

        void Game_OnlySquadLeaderSpawn(FrostbiteClient sender, bool isEnabled)
        {
            this.AppendSetting("vars.onlySquadLeaderSpawn", isEnabled.ToString());
        }

        void Game_RegenerateHealth(FrostbiteClient sender, bool isEnabled)
        {
            this.AppendSetting("vars.regenerateHealth", isEnabled.ToString());
        }

        void Game_NameTag(FrostbiteClient sender, bool isEnabled)
        {
            this.AppendSetting("vars.nameTag", isEnabled.ToString());
        }

        void Game_BulletDamage(FrostbiteClient sender, int limit)
        {
            this.AppendSetting("vars.bulletDamage", limit.ToString());
        }

        void Game_VehicleSpawnDelay(FrostbiteClient sender, int limit)
        {
            this.AppendSetting("vars.vehicleSpawnDelay", limit.ToString());
        }

        void Game_VehicleSpawnAllowed(FrostbiteClient sender, bool isEnabled)
        {
            this.AppendSetting("vars.vehicleSpawnAllowed", isEnabled.ToString());
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
            this.AppendSetting("vars.3pCam", isEnabled.ToString());
        }

        void Client_MiniMapSpotting(FrostbiteClient sender, bool isEnabled)
        {
            this.AppendSetting("vars.miniMapSpotting", isEnabled.ToString());
        }

        void Game_GameModeCounter(FrostbiteClient sender, int limit)
        {
            this.AppendSetting("vars.gameModeCounter", limit.ToString());
        }

        void Game_RoundTimeLimit(FrostbiteClient sender, int limit)
        {
            this.AppendSetting("vars.roundTimeLimit", limit.ToString());
        }

        void Game_TicketBleedRate(FrostbiteClient sender, int limit)
        {
            this.AppendSetting("vars.ticketBleedRate", limit.ToString());
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

        void Game_ReservedSlotsListAggressiveJoin(FrostbiteClient sender, bool isEnabled)
        {
            this.AppendSetting("reservedSlotsList.aggressiveJoin", isEnabled.ToString());
        }

        void Game_RoundLockdownCountdown(FrostbiteClient sender, int limit)
        {
            this.AppendSetting("vars.roundLockdownCountdown", limit.ToString());
        }

        void Game_RoundWarmupTimeout(FrostbiteClient sender, int limit)
        {
            this.AppendSetting("vars.roundWarmupTimeout", limit.ToString());
        }

        void Game_PremiumStatus(FrostbiteClient sender, bool isEnabled)
        {
            this.AppendSetting("vars.premiumStatus", isEnabled.ToString());
        }

        void Game_MaxSpectators(FrostbiteClient sender, int limit)
        {
            this.AppendSetting("vars.maxSpectators", limit.ToString());
        }

        void Game_IsHitIndicator(FrostbiteClient sender, bool isEnabled)
        {
            this.AppendSetting("vars.hitIndicatorsEnabled", isEnabled.ToString());
        }

        // todo this may be wrong. I don't know if the value is readonly even in the startup cfg.
        void Game_ServerType(FrostbiteClient sender, string value)
        {
            this.AppendSetting("vars.serverType", value);
        }

        void Game_IsCommander(FrostbiteClient sender, bool isEnabled)
        {
            this.AppendSetting("vars.commander", isEnabled.ToString());
        }

        void Game_AlwaysAllowSpectators(FrostbiteClient sender, bool isEnabled)
        {
            this.AppendSetting("vars.alwaysAllowSpectators", isEnabled.ToString());
        }

        void Game_FairFight(FrostbiteClient sender, bool isEnabled)
        {
            if (isEnabled == true)
            {
                this.AppendSetting("fairFight.activate");
            }
            else
            {
                this.AppendSetting("fairFight.deactivate");
            }
        }

        void Game_TeamFactionOverride(FrostbiteClient sender, int teamId, int faction)
        {
            switch (teamId)
            {
                case 1:
                    this.AppendSetting("vars.teamFactionOverride 1", faction.ToString());
                    break;
                case 2:
                    this.AppendSetting("vars.teamFactionOverride 2", faction.ToString());
                    break;
                case 3:
                    this.AppendSetting("vars.teamFactionOverride 3", faction.ToString());
                    break;
                case 4:
                    this.AppendSetting("vars.teamFactionOverride 4", faction.ToString());
                    break;
            }
        }
    }
}
