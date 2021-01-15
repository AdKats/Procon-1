using PRoCon.Core.Maps;
using PRoCon.Core.Players;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace PRoCon.Core.Remote
{
    public class BF4Client : BFClient
    {
        protected DateTime LastListPlayersStamp;
        protected int LastMapListOffset;
        protected int LastReservedSlotsListOffset;
        protected int LastSpectatorListListOffset;
        protected int LastGameAdminListOffset;

        public BF4Client(FrostbiteConnection connection)
            : base(connection)
        {

            RequestDelegates.Add("player.spawn", DispatchPlayerOnSpawnRequest);

            #region Maplist

            FullMapList = new List<MaplistEntry>();
            LastMapListOffset = 0;
            /*
            this.m_responseDelegates.Add("mapList.load", this.DispatchMapListLoadResponse);
            this.m_responseDelegates.Add("mapList.save", this.DispatchMapListSaveResponse );
            this.m_responseDelegates.Add("mapList.list", this.DispatchMapListListResponse);
            this.m_responseDelegates.Add("mapList.clear", this.DispatchMapListClearResponse);
            

            this.m_responseDelegates.Add("mapList.remove", this.DispatchMapListRemoveResponse);
            */
            ResponseDelegates.Add("mapList.add", DispatchMapListAppendResponse);

            #endregion

            #region Map list functions

            ResponseDelegates.Add("mapList.restartRound", DispatchAdminRestartRoundResponse);
            ResponseDelegates.Add("mapList.availableMaps", DispatchAdminSupportedMapsResponse);

            ResponseDelegates.Add("mapList.runNextRound", DispatchAdminRunNextRoundResponse);

            ResponseDelegates.Add("currentLevel", DispatchAdminCurrentLevelResponse);

            ResponseDelegates.Add("mapList.endRound", DispatchAdminEndRoundResponse);

            ResponseDelegates.Add("mapList.setNextMapIndex", DispatchMapListNextLevelIndexResponse);

            ResponseDelegates.Add("mapList.getMapIndices", DispatchMapListGetMapIndicesResponse);

            ResponseDelegates.Add("mapList.getRounds", DispatchMapListGetRoundsResponse);

            #endregion

            #region Reserved Slots

            FullReservedSlotsList = new List<String>();

            // Note: These delegates point to methods in FrostbiteClient.
            ResponseDelegates.Add("reservedSlotsList.configFile", DispatchReservedSlotsConfigFileResponse);
            ResponseDelegates.Add("reservedSlotsList.load", DispatchReservedSlotsLoadResponse);
            ResponseDelegates.Add("reservedSlotsList.save", DispatchReservedSlotsSaveResponse);
            ResponseDelegates.Add("reservedSlotsList.add", DispatchReservedSlotsAddPlayerResponse);
            ResponseDelegates.Add("reservedSlotsList.remove", DispatchReservedSlotsRemovePlayerResponse);
            ResponseDelegates.Add("reservedSlotsList.clear", DispatchReservedSlotsClearResponse);
            ResponseDelegates.Add("reservedSlotsList.list", DispatchReservedSlotsListResponse);
            ResponseDelegates.Add("reservedSlotsList.aggressiveJoin", DispatchReservedSlotsAggressiveJoinResponse);

            #endregion


            #region Reserved Slots

            FullSpectatorList = new List<String>();

            // Note: These delegates point to methods in FrostbiteClient.
            ResponseDelegates.Add("spectatorList.configFile", DispatchSpectatorListConfigFileResponse);
            ResponseDelegates.Add("spectatorList.load", DispatchSpectatorListLoadResponse);
            ResponseDelegates.Add("spectatorList.save", DispatchSpectatorListSaveResponse);
            ResponseDelegates.Add("spectatorList.add", DispatchSpectatorListAddPlayerResponse);
            ResponseDelegates.Add("spectatorList.remove", DispatchSpectatorListRemovePlayerResponse);
            ResponseDelegates.Add("spectatorList.clear", DispatchSpectatorListClearResponse);
            ResponseDelegates.Add("spectatorList.list", DispatchSpectatorListListResponse);

            #endregion

            #region Reserved Slots

            FullGameAdminList = new List<String>();

            // Note: These delegates point to methods in FrostbiteClient.
            ResponseDelegates.Add("gameAdmin.configFile", DispatchGameAdminConfigFileResponse);
            ResponseDelegates.Add("gameAdmin.load", DispatchGameAdminLoadResponse);
            ResponseDelegates.Add("gameAdmin.save", DispatchGameAdminSaveResponse);
            ResponseDelegates.Add("gameAdmin.add", DispatchGameAdminAddPlayerResponse);
            ResponseDelegates.Add("gameAdmin.remove", DispatchGameAdminRemovePlayerResponse);
            ResponseDelegates.Add("gameAdmin.clear", DispatchGameAdminClearResponse);
            ResponseDelegates.Add("gameAdmin.list", DispatchGameAdminListResponse);

            #endregion

            #region vars

            ResponseDelegates.Add("vars.autoBalance", DispatchVarsTeamBalanceResponse);
            //this.m_responseDelegates.Add("vars.noInteractivityTimeoutTime", this.DispatchVarsIdleTimeoutResponse);

            ResponseDelegates.Add("vars.maxPlayers", DispatchVarsPlayerLimitResponse);

            ResponseDelegates.Add("vars.3pCam", DispatchVarsThirdPersonVehicleCamerasResponse);

            ResponseDelegates.Add("admin.eventsEnabled", DispatchEventsEnabledResponse);

            ResponseDelegates.Add("vars.hitIndicatorsEnabled", DispatchVarsHitIndicatorsResponse);

            ResponseDelegates.Add("vars.commander", DispatchVarsCommander);
            ResponseDelegates.Add("vars.forceReloadWholeMags", DispatchVarsForceReloadWholeMags);

            ResponseDelegates.Add("vars.alwaysAllowSpectators", DispatchVarsAlwaysAllowSpectators);

            ResponseDelegates.Add("vars.serverType", DispatchVarsServerType);

            /*
            vars.clientSideDamageArbitration
            vars.killRotation
            vars.noInteractivityRoundBan
             * */

            ResponseDelegates.Add("vars.vehicleSpawnAllowed", DispatchVarsVehicleSpawnAllowedResponse);
            ResponseDelegates.Add("vars.vehicleSpawnDelay", DispatchVarsVehicleSpawnDelayResponse);
            ResponseDelegates.Add("vars.bulletDamage", DispatchVarsBulletDamageResponse);
            ResponseDelegates.Add("vars.nameTag", DispatchVarsNameTagResponse);
            ResponseDelegates.Add("vars.regenerateHealth", DispatchVarsRegenerateHealthResponse);
            ResponseDelegates.Add("vars.roundRestartPlayerCount", DispatchVarsRoundRestartPlayerCountResponse);
            ResponseDelegates.Add("vars.onlySquadLeaderSpawn", DispatchVarsOnlySquadLeaderSpawnResponse);
            ResponseDelegates.Add("vars.unlockMode", DispatchVarsUnlockModeResponse);
            ResponseDelegates.Add("vars.preset", DispatchVarsPresetResponse);
            ResponseDelegates.Add("vars.gunMasterWeaponsPreset", DispatchVarsGunMasterWeaponsPresetResponse);
            ResponseDelegates.Add("vars.soldierHealth", DispatchVarsSoldierHealthResponse);
            ResponseDelegates.Add("vars.hud", DispatchVarsHudResponse);
            ResponseDelegates.Add("vars.playerManDownTime", DispatchVarsPlayerManDownTimeResponse);
            ResponseDelegates.Add("vars.roundStartPlayerCount", DispatchVarsRoundStartPlayerCountResponse);
            ResponseDelegates.Add("vars.playerRespawnTime", DispatchVarsPlayerRespawnTimeResponse);
            ResponseDelegates.Add("vars.gameModeCounter", DispatchVarsGameModeCounterResponse);
            ResponseDelegates.Add("vars.roundTimeLimit", DispatchVarsRoundTimeLimitResponse);
            ResponseDelegates.Add("vars.ticketBleedRate", DispatchVarsTicketBleedRateResponse);
            ResponseDelegates.Add("vars.idleBanRounds", DispatchVarsIdleBanRoundsResponse);

            ResponseDelegates.Add("vars.serverMessage", DispatchVarsServerMessageResponse);

            ResponseDelegates.Add("vars.roundLockdownCountdown", DispatchVarsRoundLockdownCountdownResponse);
            ResponseDelegates.Add("vars.roundWarmupTimeout", DispatchVarsRoundWarmupTimeoutResponse);

            ResponseDelegates.Add("vars.premiumStatus", DispatchVarsPremiumStatusResponse);

            ResponseDelegates.Add("punkBuster.isActive", DispatchVarsPunkbusterResponse);

            ResponseDelegates.Add("fairFight.isActive", DispatchVarsFairFightStatusResponse);

            ResponseDelegates.Add("vars.maxSpectators", DispatchVarsMaxSpectatorsResponse);

            ResponseDelegates.Add("vars.teamKillKickForBan", DispatchVarsTeamKillKickForBanResponse);

            ResponseDelegates.Add("vars.teamFactionOverride", DispatchVarsTeamFactionOverrideResponse);

            ResponseDelegates.Add("vars.mpExperience", DispatchVarMpExperienceResponse);

            #endregion

            #region player.* / squad.* commands

            ResponseDelegates.Add("player.onAuthenticated", DispatchPlayerOnAuthenticatedRequest);
            ResponseDelegates.Add("player.idleDuration", DispatchPlayerIdleDurationResponse);
            ResponseDelegates.Add("player.isAlive", DispatchPlayerIsAliveResponse);
            ResponseDelegates.Add("player.ping", DispatchPlayerPingResponse);

            ResponseDelegates.Add("squad.leader", DispatchSquadLeaderResponse);
            ResponseDelegates.Add("squad.listActive", DispatchSquadListActiveResponse);
            ResponseDelegates.Add("squad.listPlayers", DispatchSquadListPlayersResponse);
            ResponseDelegates.Add("squad.private", DispatchSquadIsPrivateResponse);

            #endregion

            ResponseDelegates.Add("admin.help", DispatchHelpResponse);

            GetPacketsPattern = new Regex(GetPacketsPattern + @"|^reservedSlotsList.list|^player\.idleDuration|^player\.isAlive|^player.ping|squad\.listActive|^squad\.listPlayers|^squad\.private", RegexOptions.Compiled);
        }

        public override string GameType
        {
            get { return "BF4"; }
        }

        public override bool HasOpenMaplist
        {
            get
            {
                // true, does not lock maplist to a playlist
                return true;
            }
        }

        public List<MaplistEntry> FullMapList { get; private set; }

        public List<String> FullReservedSlotsList { get; private set; }

        public List<String> FullSpectatorList { get; private set; }

        public List<String> FullGameAdminList { get; private set; }

        public override void FetchStartupVariables()
        {
            base.FetchStartupVariables();

            SendGetVarsTeamKillKickForBanPacket();

            SendGetVarsPlayerLimitPacket();

            SendGetVarsIdleBanRoundsPacket();

            SendGetVarsVehicleSpawnAllowedPacket();

            SendGetVarsVehicleSpawnDelayPacket();
            SendGetVarsBulletDamagePacket();

            SendGetVarsNameTagPacket();

            SendGetVarsRegenerateHealthPacket();

            SendGetVarsRoundRestartPlayerCountPacket();

            SendGetVarsRoundStartPlayerCountPacket();
            SendGetVarsOnlySquadLeaderSpawnPacket();

            SendGetVarsUnlockModePacket();
            SendGetVarsGunMasterWeaponsPresetPacket();

            SendGetVarsSoldierHealthPacket();

            SendGetVarsHudPacket();

            SendGetVarsPlayerManDownTimePacket();

            SendGetVarsPlayerRespawnTimePacket();

            SendGetVarsGameModeCounterPacket();

            SendGetVarsServerMessagePacket();

            SendGetReservedSlotsListAggressiveJoinPacket();
            SendGetVarsRoundLockdownCountdownPacket();
            SendGetVarsRoundWarmupTimeoutPacket();

            SendSpectatorListListPacket();

            SendGameAdminListPacket();

            SendGetVarsFairFightPacket();

            SendGetVarsMaxSpectatorsPacket();

            SendGetVarsHitIndicatorsEnabled();

            SendGetVarsServerType();
            SendGetVarsCommander();
            SendGetVarsAlwaysAllowSpectators();
            SendGetVarsForceReloadWholeMags();
            SendGetVarsRoundTimeLimitPacket();
            SendGetVarsTicketBleedRatePacket();

            SendGetVarsTeamFactionOverridePacket();

            SendGetVarsPresetPacket();
            SendGetVarsMpExperiencePacket();
        }

        #region Overridden Events

        public override event ServerInfoHandler ServerInfo;

        public override event PlayerAuthenticatedHandler PlayerAuthenticated;

        public override event ListPlayersHandler ListPlayers;

        //public override event FrostbiteClient.RoundOverPlayersHandler RoundOverPlayers;

        public override event PlayerKilledHandler PlayerKilled;

        public override event PlayerSpawnedHandler PlayerSpawned;

        public override event LevelLoadedHandler LevelLoaded;

        #region Maplist

        // public override event FrostbiteClient.MapListConfigFileHandler MapListConfigFile;
        //public override event FrostbiteClient.EmptyParamterHandler MapListLoad;
        //public override event FrostbiteClient.EmptyParamterHandler MapListSave;
        public override event MapListAppendedHandler MapListMapAppended;
        //public override event FrostbiteClient.MapListLevelIndexHandler MapListNextLevelIndex;
        //public override event FrostbiteClient.MapListLevelIndexHandler MapListMapRemoved;
        public override event MapListMapInsertedHandler MapListMapInserted;
        //public override event FrostbiteClient.EmptyParamterHandler MapListCleared;
        public override event MapListListedHandler MapListListed;

        #endregion

        #region ReservedSlotsList

        public override event ReservedSlotsListHandler ReservedSlotsList;
        public override event IsEnabledHandler ReservedSlotsListAggressiveJoin;

        #endregion

        #region Spectator List

        public override event SpectatorListListHandler SpectatorListList;

        #endregion

        #region Game Admin

        public override event GameAdminListHandler GameAdminList;

        #endregion

        #region Map/Round

        //public override event FrostbiteClient.EmptyParamterHandler RunNextRound; // Alias for runNextRound
        public override event CurrentLevelHandler CurrentLevel;
        //public override event FrostbiteClient.EmptyParamterHandler RestartRound; // Alias for restartRound
        public override event SupportedMapsHandler SupportedMaps;
        //public override event FrostbiteClient.EndRoundHandler EndRound;

        #endregion

        #region vars

        public override event IsEnabledHandler VehicleSpawnAllowed;
        public override event LimitHandler VehicleSpawnDelay;

        public override event LimitHandler BulletDamage;

        public override event IsEnabledHandler NameTag;

        public override event IsEnabledHandler RegenerateHealth;

        public override event IsEnabledHandler OnlySquadLeaderSpawn;

        public override event UnlockModeHandler UnlockMode;
        public override event BF4presetHandler BF4preset;
        public override event MpExperienceHandler MpExperience;
        public override event GunMasterWeaponsPresetHandler GunMasterWeaponsPreset;

        public override event LimitHandler SoldierHealth;

        public override event IsEnabledHandler Hud;

        public override event LimitHandler PlayerManDownTime;

        public override event LimitHandler RoundRestartPlayerCount;
        public override event LimitHandler RoundStartPlayerCount;

        public override event LimitHandler PlayerRespawnTime;

        public override event LimitHandler GameModeCounter;
        public override event LimitHandler RoundTimeLimit;
        public override event LimitHandler TicketBleedRate;
        public override event LimitHandler IdleBanRounds;

        public override event ServerMessageHandler ServerMessage;

        public override event LimitHandler RoundLockdownCountdown;
        public override event LimitHandler RoundWarmupTimeout;

        public override event IsEnabledHandler PremiumStatus;

        public override event IsEnabledHandler IsHitIndicator;

        public override event IsEnabledHandler IsCommander;
        public override event IsEnabledHandler IsForceReloadWholeMags;

        public override event IsEnabledHandler AlwaysAllowSpectators;

        public override event VarsStringHandler ServerType;

        public override event TeamFactionOverrideHandler TeamFactionOverride;

        #region player/squad cmd_handler

        public override event PlayerIdleStateHandler PlayerIdleState;
        public override event PlayerIsAliveHandler PlayerIsAlive;
        public override event PlayerPingedByAdminHandler PlayerPingedByAdmin;

        public override event SquadLeaderHandler SquadLeader;
        public override event SquadListActiveHandler SquadListActive;
        public override event SquadListPlayersHandler SquadListPlayers;
        public override event SquadIsPrivateHandler SquadIsPrivate;

        #endregion

        #endregion

        #region Overridden Packet Helpers

        public override void SendEventsEnabledPacket(bool isEventsEnabled)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("admin.eventsEnabled", Packet.Bltos(isEventsEnabled));
            }
        }

        public override void SendHelpPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("admin.help");
            }
        }

        #region Map Controls

        public override void SendAdminRestartRoundPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("mapList.restartRound");
            }
        }

        public override void SendAdminRunNextRoundPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("mapList.runNextRound");
            }
        }

        public override void SendMapListNextLevelIndexPacket(int index)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("mapList.setNextMapIndex", index.ToString(CultureInfo.InvariantCulture));
            }
        }

        #endregion

        #region Maplist

        public override void SendMapListListRoundsPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("mapList.list");
            }
        }

        public virtual void SendMapListListRoundsPacket(int startIndex)
        {
            if (IsLoggedIn == true)
            {
                if (startIndex >= 0)
                {
                    BuildSendPacket("mapList.list", startIndex.ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    BuildSendPacket("mapList.list");
                }
            }
        }

        public override void SendMapListAppendPacket(MaplistEntry map)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("mapList.add", map.MapFileName, map.Gamemode, (map.Rounds > 0 ? map.Rounds : 2).ToString(CultureInfo.InvariantCulture));
            }
        }

        public override void SendMapListInsertPacket(MaplistEntry map)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("mapList.add", map.MapFileName, map.Gamemode, (map.Rounds > 0 ? map.Rounds : 2).ToString(CultureInfo.InvariantCulture), map.Index.ToString(CultureInfo.InvariantCulture));
            }
        }

        #endregion

        #region Reserved Slot List

        public override void SendReservedSlotsLoadPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("reservedSlotsList.load");
            }
        }

        public override void SendReservedSlotsListPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("reservedSlotsList.list");
            }
        }

        public virtual void SendReservedSlotsListPacket(int startIndex)
        {
            if (IsLoggedIn == true)
            {
                if (startIndex >= 0)
                {
                    BuildSendPacket("reservedSlotsList.list", startIndex.ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    BuildSendPacket("reservedSlotsList.list");
                }
            }
        }

        public virtual void SendSpectatorListListPacket(int startIndex)
        {
            if (IsLoggedIn == true)
            {
                if (startIndex >= 0)
                {
                    BuildSendPacket("spectatorList.list", startIndex.ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    BuildSendPacket("spectatorList.list");
                }
            }
        }

        public virtual void SendGameAdminListPacket(int startIndex)
        {
            if (IsLoggedIn == true)
            {
                if (startIndex >= 0)
                {
                    BuildSendPacket("gameAdmin.list", startIndex.ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    BuildSendPacket("gameAdmin.list");
                }
            }
        }

        public override void SendReservedSlotsAddPlayerPacket(string soldierName)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("reservedSlotsList.add", soldierName);
            }
        }

        public override void SendReservedSlotsRemovePlayerPacket(string soldierName)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("reservedSlotsList.remove", soldierName);
            }
        }

        public override void SendReservedSlotsSavePacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("reservedSlotsList.save");
            }
        }

        public virtual void SendReservedSlotsAggressiveJoinPacket(bool enabled)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("reservedSlotsList.aggressiveJoin", Packet.Bltos(enabled));
            }
        }

        #endregion

        #region Vars

        public override void SendSetVarsPunkBusterPacket(bool enabled)
        {
            if (IsLoggedIn == true)
            {
                if (enabled == true)
                {
                    BuildSendPacket("punkBuster.activate");
                }
                else
                {
                    BuildSendPacket("punkBuster.deactivate");
                }
            }
        }

        public override void SendGetVarsPunkBusterPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("punkBuster.isActive");
            }
        }

        public override void SendSetVarsPlayerLimitPacket(int limit)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.maxPlayers", limit.ToString(CultureInfo.InvariantCulture));
            }
        }

        public override void SendGetVarsPlayerLimitPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.maxPlayers");
            }
        }

        public override void SendSetVarsTeamBalancePacket(bool enabled)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.autoBalance", Packet.Bltos(enabled));
            }
        }


        public override void SendGetVarsTeamBalancePacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.autoBalance");
            }
        }

        public override void SendSetVarsThirdPersonVehicleCamerasPacket(bool enabled)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.3pCam", Packet.Bltos(enabled));
            }
        }

        public override void SendGetVarsThirdPersonVehicleCamerasPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.3pCam");
            }
        }


        public override void SendSetVarsAdminPasswordPacket(string password)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("admin.password", password);
            }
        }

        public override void SendGetVarsAdminPasswordPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("admin.password");
            }
        }

        public override void SendSetVarsRoundLockdownCountdownPacket(int limit)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.roundLockdownCountdown", limit.ToString(CultureInfo.InvariantCulture));
            }
        }

        public override void SendGetVarsRoundLockdownCountdownPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.roundLockdownCountdown");
            }
        }

        public override void SendSetVarsRoundWarmupTimeoutPacket(int limit)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.roundWarmupTimeout", limit.ToString(CultureInfo.InvariantCulture));
            }
        }

        public override void SendGetVarsRoundWarmupTimeoutPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.roundWarmupTimeout");
            }
        }

        public virtual void SendPremiumStatusPacket(bool enabled)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.premiumStatus", Packet.Bltos(enabled));
            }
        }

        #endregion

        #region Level Vars (Non existent in BF3)

        public override void SendLevelVarsListPacket(LevelVariableContext context)
        {
            // Do nothing!
        }

        #endregion

        #endregion

        #region R-38 player-squad Packet Helpers

        public virtual void SendPlayerIdleDurationPacket(string soldierName)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("player.idleDuration", soldierName);
            }
        }

        public virtual void SendPlayerIsAlivePacket(string soldierName)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("player.isAlive", soldierName);
            }
        }


        public virtual void SendPlayerPingPacket(string soldierName)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("player.ping", soldierName);
            }
        }

        public virtual void SendSetSquadLeaderPacket(int teamId, int squadId, string soldierName)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("squad.leader", teamId.ToString(CultureInfo.InvariantCulture), squadId.ToString(CultureInfo.InvariantCulture), soldierName);
            }
        }

        public virtual void SendGetSquadLeaderPacket(int teamId, int squadId)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("squad.leader", teamId.ToString(CultureInfo.InvariantCulture), squadId.ToString(CultureInfo.InvariantCulture));
            }
        }

        public virtual void SendSquadListActivePacket(int teamId)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("squad.listActive", teamId.ToString(CultureInfo.InvariantCulture));
            }
        }

        public virtual void SendSquadListPlayersPacket(int teamId, int squadId)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("squad.listPlayers", teamId.ToString(CultureInfo.InvariantCulture), squadId.ToString(CultureInfo.InvariantCulture));
            }
        }

        public virtual void SendSetSquadPrivatePacket(int teamId, int squadId, bool isPrivate)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("squad.private", teamId.ToString(CultureInfo.InvariantCulture), squadId.ToString(CultureInfo.InvariantCulture), Packet.Bltos(isPrivate));
            }
        }

        public virtual void SendGetSquadPrivatePacket(int teamId, int squadId)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("squad.private", teamId.ToString(CultureInfo.InvariantCulture), squadId.ToString(CultureInfo.InvariantCulture));
            }
        }

        public virtual void SendGetVarsMpExperiencePacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.mpExperience");
            }
        }

        #endregion

        #region Overridden Response Handlers

        protected override void DispatchPlayerOnJoinRequest(FrostbiteConnection sender, Packet cpRequestPacket)
        {
            base.DispatchPlayerOnJoinRequest(sender, cpRequestPacket);

            if (cpRequestPacket.Words.Count >= 3)
            {
                if (PlayerAuthenticated != null)
                {
                    this.PlayerAuthenticated(this, cpRequestPacket.Words[1], cpRequestPacket.Words[2]);
                }
            }
        }

        protected override void DispatchServerInfoResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (ServerInfo != null)
            {
                var newServerInfo = new CServerInfo(new List<string>() {
                    "ServerName",
                    "PlayerCount",
                    "MaxPlayerCount",
                    "GameMode",
                    "Map",
                    "CurrentRound",
                    "TotalRounds",
                    "TeamScores",
                    "ConnectionState",
                    "Ranked",
                    "PunkBuster",
                    "Passworded",
                    "ServerUptime",
                    "RoundTime",
                    "ExternalGameIpandPort",
                    "PunkBusterVersion",
                    "JoinQueueEnabled",
                    "ServerRegion",
                    "PingSite",
                    "ServerCountry",
                    "BlazePlayerCount",
                    "BlazeGameState"
                }, cpRecievedPacket.Words.GetRange(1, cpRecievedPacket.Words.Count - 1));

                this.ServerInfo(this, newServerInfo);
            }
        }

        protected override void DispatchAdminListPlayersResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 2)
            {
                cpRecievedPacket.Words.RemoveAt(0);
                if (ListPlayers != null)
                {
                    List<CPlayerInfo> lstPlayers = CPlayerInfo.GetPlayerList(cpRecievedPacket.Words);
                    var cpsSubset = new CPlayerSubset(cpRequestPacket.Words.GetRange(1, cpRequestPacket.Words.Count - 1));

                    this.ListPlayers(this, lstPlayers, cpsSubset);

                    if (IsLayered == false)
                    {
                        if ((DateTime.Now - LastListPlayersStamp).TotalSeconds >= 30)
                        {
                            LastListPlayersStamp = DateTime.Now;
                            // fire pings on each player but not if ping comes with listPlayers
                            bool doPing = true;
                            if (lstPlayers.Count > 0)
                            {
                                if (lstPlayers[0].Ping != 0)
                                {
                                    doPing = false;
                                }
                            }
                            if (doPing == true)
                            {
                                foreach (CPlayerInfo cpiPlayer in lstPlayers)
                                {
                                    SendPlayerPingPacket(cpiPlayer.SoldierName);
                                }
                            }
                        }
                    }
                    lstPlayers.Clear();
                }
            }
        }

        protected override void DispatchPlayerOnKillRequest(FrostbiteConnection sender, Packet cpRequestPacket)
        {
            if (PlayerKilled != null)
            {
                bool headshot = false;
                if (bool.TryParse(cpRequestPacket.Words[4], out headshot) == true)
                {
                    this.PlayerKilled(this, cpRequestPacket.Words[1], cpRequestPacket.Words[2], cpRequestPacket.Words[3], headshot, new Point3D(0, 0, 0), new Point3D(0, 0, 0));
                }
            }
        }

        protected override void DispatchPlayerOnSpawnRequest(FrostbiteConnection sender, Packet cpRequestPacket)
        {
            //if (cpRequestPacket.Words.Count >= 9) {
            if (PlayerSpawned != null)
            {
                this.PlayerSpawned(this, cpRequestPacket.Words[1], "", new List<string>() {
                    "",
                    "",
                    ""
                }, new List<string>() {
                    "",
                    "",
                    ""
                }); // new Inventory(cpRequestPacket.Words[3], cpRequestPacket.Words[4], cpRequestPacket.Words[5], cpRequestPacket.Words[6], cpRequestPacket.Words[7], cpRequestPacket.Words[8]));
            }
            //}
        }

        protected override void DispatchServerOnLevelLoadedRequest(FrostbiteConnection sender, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 5)
            {
                if (LevelLoaded != null)
                {
                    int iRoundsPlayed = 0, iRoundsTotal = 0;

                    if (int.TryParse(cpRequestPacket.Words[3], out iRoundsPlayed) == true && int.TryParse(cpRequestPacket.Words[4], out iRoundsTotal) == true)
                    {
                        this.LevelLoaded(this, cpRequestPacket.Words[1], cpRequestPacket.Words[2], iRoundsPlayed, iRoundsTotal);
                        SendGetVarsTeamFactionOverridePacket();
                    }
                }
            }
        }

        #region Map Functions

        protected override void DispatchAdminCurrentLevelResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1 && cpRecievedPacket.Words.Count >= 2)
            {
                if (CurrentLevel != null)
                {
                    this.CurrentLevel(this, cpRecievedPacket.Words[1]);
                }
            }
        }

        protected override void DispatchAdminSupportedMapsResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            /*
            if (cpRequestPacket.Words.Count >= 2 && cpRecievedPacket.Words.Count > 1) {
                if (this.SupportedMaps != null) {
                    this.SupportedMaps(this, cpRequestPacket.Words[1], cpRecievedPacket.Words.GetRange(1, cpRecievedPacket.Words.Count - 1));
                }
            }
            */
        }

        #endregion

        #region Map List

        protected override void DispatchMapListListResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                // start try for mapList.list offset fetch
                int iRequestStartOffset = 0;
                int iNumReturnedMaps = 0;

                if (cpRequestPacket.Words.Count >= 2)
                {
                    if (int.TryParse(cpRequestPacket.Words[1], out iRequestStartOffset) == false)
                    {
                        iRequestStartOffset = 0;
                    }
                }

                if (int.TryParse(cpRecievedPacket.Words[0], out iNumReturnedMaps) == false)
                {
                    iNumReturnedMaps = 0;
                }
                // end

                cpRecievedPacket.Words.RemoveAt(0);

                if (iRequestStartOffset == 0)
                {
                    FullMapList.Clear();
                    LastMapListOffset = 0;
                }
                else
                {
                    if (LastMapListOffset < iRequestStartOffset)
                    {
                        LastMapListOffset = iRequestStartOffset;
                    }
                    else
                    {
                        return;
                    }
                }

                #region parse mapList.List output

                var lstMaplist = new List<MaplistEntry>();

                int maps = 0;
                int parameters = 0;

                if (int.TryParse(cpRecievedPacket.Words[0], out maps) == true && int.TryParse(cpRecievedPacket.Words[1], out parameters) == true)
                {
                    for (int mapOffset = 0; mapOffset < maps; mapOffset++)
                    {
                        string mapFileName = "";
                        string gameMode = "";
                        int rounds = 0;

                        for (int parameterOffset = 0; parameterOffset < parameters; parameterOffset++)
                        {
                            string data = cpRecievedPacket.Words[2 + (mapOffset * parameters) + parameterOffset];

                            switch (parameterOffset)
                            {
                                case 0:
                                    mapFileName = data;
                                    break;
                                case 1:
                                    gameMode = data;
                                    break;
                                case 2:
                                    int.TryParse(data, out rounds);
                                    break;
                            }
                        }

                        lstMaplist.Add(new MaplistEntry(gameMode, mapFileName, rounds));
                    }

                    #region oldStuff

                    /*
                    int rounds = 0;

                    for (int i = 0; i < cpRecievedPacket.Words.Count; i = i + 3) {
                        if (int.TryParse(cpRecievedPacket.Words[i + 2], out rounds) == true) {

                            Match temp = Regex.Match(cpRecievedPacket.Words[i], @"Levels\/.*?\/(.*)");

                            lstMaplist.Add(new MaplistEntry(cpRecievedPacket.Words[i + 1], temp.Groups[1].Value, rounds));
                        }
                    }
                    */

                    #endregion // oldStuff
                }

                #endregion //parse mapList.List output

                if (lstMaplist.Count > 0)
                {
                    FullMapList.AddRange(lstMaplist);

                    SendMapListListRoundsPacket(iRequestStartOffset + 100);
                }
                else
                {
                    // original
                    if (MapListListed != null)
                    {
                        this.MapListListed(this, FullMapList);
                    }
                }
            }
        }

        protected override void DispatchMapListAppendResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 4)
            {
                MaplistEntry mapEntry = null;

                int rounds = 0;
                int index = -1;

                if (int.TryParse(cpRequestPacket.Words[3], out rounds) == true)
                {
                    mapEntry = new MaplistEntry(cpRequestPacket.Words[2], cpRequestPacket.Words[1], rounds);
                }

                if (cpRequestPacket.Words.Count >= 5 && int.TryParse(cpRequestPacket.Words[4], out index) == true)
                {
                    mapEntry = new MaplistEntry(cpRequestPacket.Words[2], cpRequestPacket.Words[1], rounds, index);
                }

                if (index == -1 && this.MapListMapAppended != null)
                {
                    this.MapListMapAppended(this, mapEntry);
                }
                else if (this.MapListMapInserted != null)
                {
                    this.MapListMapInserted(this, mapEntry);
                }
            }
        }

        protected override void DispatchMapListInsertResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 4)
            {
                int iMapIndex = 0, iRounds = 0;
                if (int.TryParse(cpRequestPacket.Words[1], out iMapIndex) == true && int.TryParse(cpRequestPacket.Words[3], out iRounds) == true)
                {
                    if (MapListMapInserted != null)
                    {
                        this.MapListMapInserted(this, new MaplistEntry(iMapIndex, cpRequestPacket.Words[2], iRounds));
                    }
                }
            }
        }

        #endregion

        #region reservedSlotsList.list

        protected override void DispatchReservedSlotsListResponse(FrostbiteConnection sender, Packet recievedPacket, Packet requestPacket)
        {
            if (requestPacket.Words.Count >= 1)
            {
                int requestStartOffset = 0;

                if (requestPacket.Words.Count >= 2)
                {
                    if (int.TryParse(requestPacket.Words[1], out requestStartOffset) == false)
                    {
                        requestStartOffset = 0;
                    }
                }
                //
                /* if (iRequestStartOffset == 0)
                {
                    this.lstFullReservedSlotsList.Clear();
                } */
                if (requestStartOffset == 0)
                {
                    FullReservedSlotsList.Clear();
                    LastReservedSlotsListOffset = 0;
                }
                else
                {
                    if (LastReservedSlotsListOffset < requestStartOffset)
                    {
                        LastReservedSlotsListOffset = requestStartOffset;
                    }
                    else
                    {
                        return;
                    }
                }

                recievedPacket.Words.RemoveAt(0);
                // List<String> lstReservedSlotsList = new List<String>();

                if (recievedPacket.Words.Count > 0)
                {
                    FullReservedSlotsList.AddRange(recievedPacket.Words);

                    SendReservedSlotsListPacket(requestStartOffset + 100);
                }
                else
                {
                    // original
                    if (ReservedSlotsList != null)
                    {
                        this.ReservedSlotsList(this, FullReservedSlotsList);
                    }
                }
            }
        }

        #endregion

        #region spectatorList.list

        protected override void DispatchSpectatorListListResponse(FrostbiteConnection sender, Packet recievedPacket, Packet requestPacket)
        {
            if (requestPacket.Words.Count >= 1)
            {
                int requestStartOffset = 0;

                if (requestPacket.Words.Count >= 2)
                {
                    if (int.TryParse(requestPacket.Words[1], out requestStartOffset) == false)
                    {
                        requestStartOffset = 0;
                    }
                }

                if (requestStartOffset == 0)
                {
                    this.FullSpectatorList.Clear();
                    LastSpectatorListListOffset = 0;
                }
                else
                {
                    if (LastSpectatorListListOffset < requestStartOffset)
                    {
                        LastSpectatorListListOffset = requestStartOffset;
                    }
                    else
                    {
                        return;
                    }
                }

                recievedPacket.Words.RemoveAt(0);

                /*
                if (recievedPacket.Words.Count > 0) {
                    FullSpectatorList.AddRange(recievedPacket.Words);

                    SendSpectatorListListPacket(requestStartOffset + 100);
                }
                else {
                    // original
                    if (SpectatorListList != null) {
                        this.SpectatorListList(this, FullSpectatorList);
                    }
                }
                */
                if (SpectatorListList != null)
                {
                    this.SpectatorListList(this, FullSpectatorList);
                }

                // Begin Temporary for lack of index support

                FullSpectatorList.AddRange(recievedPacket.Words);

                if (SpectatorListList != null)
                {
                    this.SpectatorListList(this, FullSpectatorList);
                }

                // End Temporary for lack of index support
            }
        }

        #endregion

        #region gameAdmin.list

        protected override void DispatchGameAdminListResponse(FrostbiteConnection sender, Packet recievedPacket, Packet requestPacket)
        {
            if (requestPacket.Words.Count >= 1)
            {
                int requestStartOffset = 0;

                if (requestPacket.Words.Count >= 2)
                {
                    if (int.TryParse(requestPacket.Words[1], out requestStartOffset) == false)
                    {
                        requestStartOffset = 0;
                    }
                }

                if (requestStartOffset == 0)
                {
                    FullGameAdminList.Clear();
                    LastGameAdminListOffset = 0;
                }
                else
                {
                    if (LastGameAdminListOffset < requestStartOffset)
                    {
                        LastGameAdminListOffset = requestStartOffset;
                    }
                    else
                    {
                        return;
                    }
                }

                recievedPacket.Words.RemoveAt(0);

                /*
                if (recievedPacket.Words.Count > 0) {
                    FullGameAdminList.AddRange(recievedPacket.Words);

                    SendGameAdminListPacket(requestStartOffset + 100);
                }
                else {
                    // original
                    if (GameAdminList != null) {
                        this.GameAdminList(this, FullGameAdminList);
                    }
                }
                 */

                // Begin Temporary for lack of index support

                FullGameAdminList.AddRange(recievedPacket.Words);

                if (GameAdminList != null)
                {
                    this.GameAdminList(this, FullGameAdminList);
                }

                // End Temporary for lack of index support
            }
        }

        #endregion

        #region Vars

        protected virtual void DispatchVarsBulletDamageResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (BulletDamage != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.BulletDamage(this, Convert.ToInt32(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.BulletDamage(this, Convert.ToInt32(cpRequestPacket.Words[1]));
                    }
                }
            }
        }


        protected virtual void DispatchVarsSoldierHealthResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (SoldierHealth != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.SoldierHealth(this, Convert.ToInt32(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.SoldierHealth(this, Convert.ToInt32(cpRequestPacket.Words[1]));
                    }
                }
            }
        }

        protected virtual void DispatchVarsPlayerManDownTimeResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (PlayerManDownTime != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.PlayerManDownTime(this, Convert.ToInt32(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.PlayerManDownTime(this, Convert.ToInt32(cpRequestPacket.Words[1]));
                    }
                }
            }
        }

        protected virtual void DispatchVarsRoundRestartPlayerCountResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (RoundRestartPlayerCount != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.RoundRestartPlayerCount(this, Convert.ToInt32(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.RoundRestartPlayerCount(this, Convert.ToInt32(cpRequestPacket.Words[1]));
                    }
                }
            }
        }

        protected virtual void DispatchVarsRoundStartPlayerCountResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (RoundStartPlayerCount != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.RoundStartPlayerCount(this, Convert.ToInt32(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.RoundStartPlayerCount(this, Convert.ToInt32(cpRequestPacket.Words[1]));
                    }
                }
            }
        }

        protected virtual void DispatchVarsPlayerRespawnTimeResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (PlayerRespawnTime != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.PlayerRespawnTime(this, Convert.ToInt32(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.PlayerRespawnTime(this, Convert.ToInt32(cpRequestPacket.Words[1]));
                    }
                }
            }
        }

        protected virtual void DispatchVarsGameModeCounterResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (GameModeCounter != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.GameModeCounter(this, Convert.ToInt32(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.GameModeCounter(this, Convert.ToInt32(cpRequestPacket.Words[1]));
                    }
                }
            }
        }

        protected virtual void DispatchVarsRoundTimeLimitResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (RoundTimeLimit != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.RoundTimeLimit(this, Convert.ToInt32(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.RoundTimeLimit(this, Convert.ToInt32(cpRequestPacket.Words[1]));
                    }
                }
            }
        }

        protected virtual void DispatchVarsTicketBleedRateResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (TicketBleedRate != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.TicketBleedRate(this, Convert.ToInt32(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.TicketBleedRate(this, Convert.ToInt32(cpRequestPacket.Words[1]));
                    }
                }
            }
        }

        protected virtual void DispatchVarsIdleBanRoundsResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (IdleBanRounds != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.IdleBanRounds(this, Convert.ToInt32(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.IdleBanRounds(this, Convert.ToInt32(cpRequestPacket.Words[1]));
                    }
                }
            }
        }

        protected virtual void DispatchVarsVehicleSpawnAllowedResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (VehicleSpawnAllowed != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.VehicleSpawnAllowed(this, Convert.ToBoolean(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.VehicleSpawnAllowed(this, Convert.ToBoolean(cpRequestPacket.Words[1]));
                    }
                }
            }
        }


        protected virtual void DispatchVarsVehicleSpawnDelayResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (VehicleSpawnDelay != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.VehicleSpawnDelay(this, Convert.ToInt32(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.VehicleSpawnDelay(this, Convert.ToInt32(cpRequestPacket.Words[1]));
                    }
                }
            }
        }

        protected virtual void DispatchVarsNameTagResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (NameTag != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.NameTag(this, Convert.ToBoolean(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.NameTag(this, Convert.ToBoolean(cpRequestPacket.Words[1]));
                    }
                }
            }
        }

        protected virtual void DispatchVarsRegenerateHealthResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (RegenerateHealth != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.RegenerateHealth(this, Convert.ToBoolean(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.RegenerateHealth(this, Convert.ToBoolean(cpRequestPacket.Words[1]));
                    }
                }
            }
        }

        protected virtual void DispatchVarsOnlySquadLeaderSpawnResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (OnlySquadLeaderSpawn != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.OnlySquadLeaderSpawn(this, Convert.ToBoolean(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.OnlySquadLeaderSpawn(this, Convert.ToBoolean(cpRequestPacket.Words[1]));
                    }
                }
            }
        }

        protected virtual void DispatchVarsUnlockModeResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (UnlockMode != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.UnlockMode(this, Convert.ToString(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.UnlockMode(this, Convert.ToString(cpRequestPacket.Words[1]));
                    }
                }
            }
        }

        protected virtual void DispatchVarsPresetResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (BF4preset != null)
                {
                    if (cpRecievedPacket.Words.Count == 3)
                    {
                        this.BF4preset(this, Convert.ToString(cpRecievedPacket.Words[1]), Convert.ToBoolean(cpRecievedPacket.Words[2]));
                    }
                    else if (cpRequestPacket.Words.Count >= 3)
                    {
                        this.BF4preset(this, Convert.ToString(cpRequestPacket.Words[1]), Convert.ToBoolean(cpRequestPacket.Words[2]));
                    }
                }
            }
        }

        protected virtual void DispatchVarsGunMasterWeaponsPresetResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (VehicleSpawnDelay != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.GunMasterWeaponsPreset(this, Convert.ToInt32(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.GunMasterWeaponsPreset(this, Convert.ToInt32(cpRequestPacket.Words[1]));
                    }
                }
            }
        }

        protected virtual void DispatchVarsHudResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (Hud != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.Hud(this, Convert.ToBoolean(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.Hud(this, Convert.ToBoolean(cpRequestPacket.Words[1]));
                    }
                }
            }
        }

        protected virtual void DispatchVarsServerMessageResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (ServerMessage != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.ServerMessage(this, cpRecievedPacket.Words[1]);
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.ServerMessage(this, cpRequestPacket.Words[1]);
                    }
                }
            }
        }

        #endregion

        #region ReservedSlotsList.AggressiveJoin

        protected virtual void DispatchReservedSlotsAggressiveJoinResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (ReservedSlotsListAggressiveJoin != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.ReservedSlotsListAggressiveJoin(this, Convert.ToBoolean(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.ReservedSlotsListAggressiveJoin(this, Convert.ToBoolean(cpRequestPacket.Words[1]));
                    }
                }
            }
        }

        #endregion

        #region RoundLockdownCountdown & RoundWarmupTimeout

        protected virtual void DispatchVarsRoundLockdownCountdownResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (RoundLockdownCountdown != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.RoundLockdownCountdown(this, Convert.ToInt32(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.RoundLockdownCountdown(this, Convert.ToInt32(cpRequestPacket.Words[1]));
                    }
                }
            }
        }

        protected virtual void DispatchVarsRoundWarmupTimeoutResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (RoundWarmupTimeout != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.RoundWarmupTimeout(this, Convert.ToInt32(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.RoundWarmupTimeout(this, Convert.ToInt32(cpRequestPacket.Words[1]));
                    }
                }
            }
        }

        #endregion

        #region PremiumStatus

        protected virtual void DispatchVarsPremiumStatusResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (PremiumStatus != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.PremiumStatus(this, Convert.ToBoolean(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.PremiumStatus(this, Convert.ToBoolean(cpRequestPacket.Words[1]));
                    }
                }
            }
        }

        #endregion




        protected virtual void DispatchVarsServerType(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (ServerType != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.ServerType(this, cpRecievedPacket.Words[1]);
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.ServerType(this, cpRequestPacket.Words[1]);
                    }
                }
            }
        }

        protected virtual void DispatchVarsForceReloadWholeMags(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (IsForceReloadWholeMags != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.IsForceReloadWholeMags(this, Convert.ToBoolean(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.IsForceReloadWholeMags(this, Convert.ToBoolean(cpRequestPacket.Words[1]));
                    }
                }
            }
        }

        protected virtual void DispatchVarsCommander(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (IsCommander != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.IsCommander(this, Convert.ToBoolean(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.IsCommander(this, Convert.ToBoolean(cpRequestPacket.Words[1]));
                    }
                }
            }
        }

        protected virtual void DispatchVarsHitIndicatorsResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (IsHitIndicator != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.IsHitIndicator(this, Convert.ToBoolean(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.IsHitIndicator(this, Convert.ToBoolean(cpRequestPacket.Words[1]));
                    }
                }
            }
        }

        protected virtual void DispatchVarsAlwaysAllowSpectators(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (AlwaysAllowSpectators != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.AlwaysAllowSpectators(this, Convert.ToBoolean(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.AlwaysAllowSpectators(this, Convert.ToBoolean(cpRequestPacket.Words[1]));
                    }
                }
            }
        }

        #region TeamFactionOverride

        protected virtual void DispatchVarsTeamFactionOverrideResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (TeamFactionOverride != null)
                {
                    if (cpRecievedPacket.Words.Count == 5)
                    {
                        this.TeamFactionOverride(this, 1, Convert.ToInt32(cpRecievedPacket.Words[1]));
                        this.TeamFactionOverride(this, 2, Convert.ToInt32(cpRecievedPacket.Words[2]));
                        this.TeamFactionOverride(this, 3, Convert.ToInt32(cpRecievedPacket.Words[3]));
                        this.TeamFactionOverride(this, 4, Convert.ToInt32(cpRecievedPacket.Words[4]));
                    }
                    else if (cpRequestPacket.Words.Count == 3)
                    {
                        this.TeamFactionOverride(this, Convert.ToInt32(cpRequestPacket.Words[1]), Convert.ToInt32(cpRequestPacket.Words[2]));
                    }
                }
            }
        }

        #endregion

        #region mpExperience

        protected virtual void DispatchVarMpExperienceResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (this.MpExperience != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.MpExperience(this, cpRecievedPacket.Words[1]);
                    }
                }
            }
        }

        #endregion

        #endregion

        #region R-38 player-squad command Response Handlers

        #region player.onAuthenticated
        
        protected override void DispatchPlayerOnAuthenticatedRequest(FrostbiteConnection sender, Packet cpRequestPacket)
        {
            base.DispatchPlayerOnAuthenticatedRequest(sender, cpRequestPacket);

            if (cpRequestPacket.Words.Count >= 2)
            {
                if (PlayerJoin != null)
                {
                    this.PlayerJoin(this, cpRequestPacket.Words[1]);
                }
            }
        }
        
        #endregion

        #region player.idleDuration

        protected virtual void DispatchPlayerIdleDurationResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (PlayerIdleState != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        // CultureInfo ci_neutral = new CultureInfo("en-US");
                        //int idleTime = (int)decimal.Parse(cpRecievedPacket.Words[1], ci_neutral);
                        var idleTime = (int)decimal.Parse(cpRecievedPacket.Words[1], CultureInfo.InvariantCulture.NumberFormat);
                        this.PlayerIdleState(this, cpRequestPacket.Words[1], idleTime);
                    }
                }
            }
        }

        #endregion

        #region player.isAlive

        protected virtual void DispatchPlayerIsAliveResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (PlayerIsAlive != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.PlayerIsAlive(this, cpRequestPacket.Words[1], Convert.ToBoolean(cpRecievedPacket.Words[1]));
                    }
                }
            }
        }

        #endregion

        #region player.ping

        protected virtual void DispatchPlayerPingResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (PlayerPingedByAdmin != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        int ping = int.Parse(cpRecievedPacket.Words[1]);
                        if (ping == 65535)
                        {
                            ping = -1;
                        }
                        this.PlayerPingedByAdmin(this, cpRequestPacket.Words[1], ping);
                    }
                }
            }
        }

        #endregion

        #region squad.leader

        protected virtual void DispatchSquadLeaderResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 3)
            {
                if (SquadLeader != null)
                {
                    int teamId;
                    int squadId;
                    if (int.TryParse(cpRequestPacket.Words[1], out teamId) == true && int.TryParse(cpRequestPacket.Words[2], out squadId) == true)
                    {
                        string soldierName;
                        if (cpRecievedPacket.Words.Count == 2)
                        {
                            soldierName = cpRecievedPacket.Words[1];
                            this.SquadLeader(this, teamId, squadId, soldierName);
                        }
                        else if (cpRecievedPacket.Words.Count == 1)
                        {
                            soldierName = cpRequestPacket.Words[3];
                            this.SquadLeader(this, teamId, squadId, soldierName);
                        }
                    }
                }
            }
        }

        #endregion

        #region squad.listActive

        protected virtual void DispatchSquadListActiveResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 2)
            {
                if (SquadListActive != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        var squadList = new List<int>();
                        this.SquadListActive(this, Convert.ToInt32(cpRequestPacket.Words[1]), Convert.ToInt32(cpRecievedPacket.Words[1]), squadList);
                    }
                    else if (cpRecievedPacket.Words.Count >= 2)
                    {
                        var squadList = new List<int>();
                        for (int i = 2; i < cpRecievedPacket.Words.Count; i++)
                        {
                            int value;
                            if (int.TryParse(cpRecievedPacket.Words[i], out value) == true)
                            {
                                squadList.Add(value);
                            }
                        }
                        this.SquadListActive(this, Convert.ToInt32(cpRequestPacket.Words[1]), Convert.ToInt32(cpRecievedPacket.Words[1]), squadList);
                    }
                }
            }
        }

        #endregion

        #region squad.listPlayers

        protected virtual void DispatchSquadListPlayersResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 3)
            {
                if (SquadListPlayers != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        var playersInSquad = new List<String>();
                        this.SquadListPlayers(this, Convert.ToInt32(cpRequestPacket.Words[1]), Convert.ToInt32(cpRequestPacket.Words[2]), Convert.ToInt32(cpRecievedPacket.Words[1]), playersInSquad);
                    }
                    else if (cpRecievedPacket.Words.Count >= 2)
                    {
                        var playersInSquad = new List<string>();
                        for (int i = 2; i < cpRecievedPacket.Words.Count; i++)
                        {
                            playersInSquad.Add(cpRecievedPacket.Words[i]);
                        }
                        this.SquadListPlayers(this, Convert.ToInt32(cpRequestPacket.Words[1]), Convert.ToInt32(cpRequestPacket.Words[2]), Convert.ToInt32(cpRecievedPacket.Words[1]), playersInSquad);
                    }
                }
            }
        }

        #endregion

        #region squad.private

        protected virtual void DispatchSquadIsPrivateResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 3)
            {
                if (SquadIsPrivate != null)
                {
                    int teamId;
                    int squadId;
                    if (int.TryParse(cpRequestPacket.Words[1], out teamId) == true && int.TryParse(cpRequestPacket.Words[2], out squadId) == true)
                    {
                        bool isPrivate;
                        if (cpRecievedPacket.Words.Count == 2)
                        {
                            isPrivate = Convert.ToBoolean(cpRecievedPacket.Words[1]);
                            this.SquadIsPrivate(this, teamId, squadId, isPrivate);
                        }
                        else if (cpRecievedPacket.Words.Count == 1)
                        {
                            isPrivate = Convert.ToBoolean(cpRequestPacket.Words[3]);
                            this.SquadIsPrivate(this, teamId, squadId, isPrivate);
                        }
                    }
                }
            }
        }

        #endregion

        #endregion

        // public override event FrostbiteClient.RawChatHandler Chat;
        // public override event FrostbiteClient.GlobalChatHandler GlobalChat;

        #endregion
    }
}
