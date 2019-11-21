using PRoCon.Core.Maps;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace PRoCon.Core.Remote
{
    public class MOHWClient : BFClient
    {
        protected int LastMapListOffset;
        protected int LastReservedSlotsListOffset;

        public MOHWClient(FrostbiteConnection connection) : base(connection)
        {
            // Geoff Green 08/10/2011 - bug in BF3 OB version E, sent through with capital P.
            //this.m_requestDelegates.Add("PunkBuster.onMessage", this.DispatchPunkBusterOnMessageRequest);

            // Geoff Green 08/10/2011 - bug in BF3 OB version E, sent through as 'player.spawn'.
            RequestDelegates.Add("player.spawn", DispatchPlayerOnSpawnRequest);

            // MoHW R-6 crippled chat
            // this.m_requestDelegates.Add("player.onChat", this.DispatchPlayerOnChatRequest);

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

            #region vars

            ResponseDelegates.Add("vars.autoBalance", DispatchVarsTeamBalanceResponse);
            //this.m_responseDelegates.Add("vars.noInteractivityTimeoutTime", this.DispatchVarsIdleTimeoutResponse);

            ResponseDelegates.Add("vars.maxPlayers", DispatchVarsPlayerLimitResponse);

            ResponseDelegates.Add("vars.3pCam", DispatchVarsThirdPersonVehicleCamerasResponse);

            ResponseDelegates.Add("admin.eventsEnabled", DispatchEventsEnabledResponse);

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
            ResponseDelegates.Add("vars.soldierHealth", DispatchVarsSoldierHealthResponse);
            ResponseDelegates.Add("vars.hud", DispatchVarsHudResponse);
            ResponseDelegates.Add("vars.playerManDownTime", DispatchVarsPlayerManDownTimeResponse);
            ResponseDelegates.Add("vars.roundStartPlayerCount", DispatchVarsRoundStartPlayerCountResponse);
            ResponseDelegates.Add("vars.playerRespawnTime", DispatchVarsPlayerRespawnTimeResponse);
            ResponseDelegates.Add("vars.gameModeCounter", DispatchVarsGameModeCounterResponse);
            ResponseDelegates.Add("vars.idleBanRounds", DispatchVarsIdleBanRoundsResponse);

            ResponseDelegates.Add("vars.serverMessage", DispatchVarsServerMessageResponse);

            ResponseDelegates.Add("vars.roundLockdownCountdown", DispatchVarsRoundLockdownCountdownResponse);
            ResponseDelegates.Add("vars.roundWarmupTimeout", DispatchVarsRoundWarmupTimeoutResponse);

            ResponseDelegates.Add("vars.premiumStatus", DispatchVarsPremiumStatusResponse);

            #endregion

            #region MoHW vars

            // deprecated R-5 this.m_responseDelegates.Add("vars.allUnlocksUnlocked", this.DispatchVarsAllUnlocksUnlockedResponse);
            ResponseDelegates.Add("vars.buddyOutline", DispatchVarsBuddyOutlineResponse);
            ResponseDelegates.Add("vars.hudBuddyInfo", DispatchVarsHudBuddyInfoResponse);
            ResponseDelegates.Add("vars.hudClassAbility", DispatchVarsHudClassAbilityResponse);
            ResponseDelegates.Add("vars.hudCrosshair", DispatchVarsHudCrosshairResponse);
            ResponseDelegates.Add("vars.hudEnemyTag", DispatchVarsHudEnemyTagResponse);
            ResponseDelegates.Add("vars.hudExplosiveIcons", DispatchVarsHudExplosiveIconsResponse);
            ResponseDelegates.Add("vars.hudGameMode", DispatchVarsHudGameModeResponse);
            ResponseDelegates.Add("vars.hudHealthAmmo", DispatchVarsHudHealthAmmoResponse);
            ResponseDelegates.Add("vars.hudMinimap", DispatchVarsHudMinimapResponse);
            ResponseDelegates.Add("vars.hudObiturary", DispatchVarsHudObituraryResponse);
            ResponseDelegates.Add("vars.hudPointsTracker", DispatchVarsHudPointsTrackerResponse);
            ResponseDelegates.Add("vars.hudUnlocks", DispatchVarsHudUnlocksResponse);
            ResponseDelegates.Add("vars.playlist", DispatchVarsPlaylistResponse);
            ResponseDelegates.Add("plManager.setPlaylist", DispatchVarsPlaylistResponse);

            #endregion

            ResponseDelegates.Add("admin.help", DispatchHelpResponse);

            GetPacketsPattern = new Regex(GetPacketsPattern + "|^reservedSlotsList.list", RegexOptions.Compiled);
        }

        public override string GameType
        {
            get { return "MOHW"; }
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

        public override void FetchStartupVariables()
        {
            // base.FetchStartupVariables();

            #region Base fetch

            SendGetVarsBannerUrlPacket();

            SendGetVarsRankedPacket();
            SendGetVarsPlaylistPacket();
            base.FetchStartupVariablesBase();
            SendGetVarsTeamBalancePacket();
            SendGetVarsKillCamPacket();
            SendGetVarsThirdPersonVehicleCamerasPacket();

            #endregion

            #region MoHW global & R3 disabled

            // R-5 this.SendGetVarsAllUnlocksUnlockedPacket();
            // this.SendGetVarsVehicleSpawnAllowedPacket();
            // this.SendGetVarsVehicleSpawnDelayPacket();
            // this.SendGetVarsNameTagPacket();
            // this.SendGetVarsOnlySquadLeaderSpawnPacket();
            // this.SendGetVarsUnlockModePacket();
            // this.SendGetVarsHudPacket();
            // this.SendGetReservedSlotsListAggressiveJoinPacket();
            // this.SendGetVarsPremiumStatusPacket();
            // this.SendGetVarsRoundLockdownCountdownPacket();
            // this.SendGetVarsRoundWarmupTimeoutPacket();

            #endregion

            #region vars MoHW

            SendGetVarsBuddyOutlinePacket();
            SendGetVarsHudBuddyInfoPacket();
            SendGetVarsHudClassAbilityPacket();
            SendGetVarsHudCrosshairPacket();
            SendGetVarsHudEnemyTagPacket();
            SendGetVarsHudExplosiveIconsPacket();
            SendGetVarsHudGameModePacket();
            SendGetVarsHudHealthAmmoPacket();
            SendGetVarsHudMinimapPacket();
            SendGetVarsHudObituraryPacket();
            SendGetVarsHudPointsTrackerPacket();
            SendGetVarsHudUnlocksPacket();
            // see above this.SendGetVarsPlaylistPacket();

            #endregion

            #region BF3

            SendGetVarsPlayerLimitPacket();

            SendGetVarsIdleBanRoundsPacket();
            SendGetVarsBulletDamagePacket();

            SendGetVarsGameModeCounterPacket();

            SendGetVarsPlayerManDownTimePacket();
            SendGetVarsPlayerRespawnTimePacket();
            SendGetVarsRegenerateHealthPacket();

            SendGetVarsRoundRestartPlayerCountPacket();
            SendGetVarsRoundStartPlayerCountPacket();

            SendGetVarsServerMessagePacket();
            SendGetVarsSoldierHealthPacket();

            #endregion
        }

        #region Overridden Events

        public override event ServerInfoHandler ServerInfo;

        public override event PlayerAuthenticatedHandler PlayerAuthenticated;

        //public override event FrostbiteClient.ListPlayersHandler ListPlayers;

        //public override event FrostbiteClient.RoundOverPlayersHandler RoundOverPlayers;

        public override event PlayerKilledHandler PlayerKilled;

        public override event PlayerSpawnedHandler PlayerSpawned;

        // MoHW R-6 crippled chat related
        public override event RawChatHandler Chat;
        public override event GlobalChatHandler GlobalChat;

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
        public override event LimitHandler SoldierHealth;
        public override event IsEnabledHandler Hud;
        public override event LimitHandler PlayerManDownTime;
        public override event LimitHandler RoundRestartPlayerCount;
        public override event LimitHandler RoundStartPlayerCount;
        public override event LimitHandler PlayerRespawnTime;
        public override event LimitHandler GameModeCounter;
        public override event LimitHandler IdleBanRounds;
        public override event ServerMessageHandler ServerMessage;
        public override event LimitHandler RoundLockdownCountdown;
        public override event LimitHandler RoundWarmupTimeout;
        public override event IsEnabledHandler PremiumStatus;

        #endregion

        #region vars MoHW

        // R-5 public override event FrostbiteClient.IsEnabledHandler AllUnlocksUnlocked;
        public override event IsEnabledHandler BuddyOutline;
        public override event IsEnabledHandler HudBuddyInfo;
        public override event IsEnabledHandler HudClassAbility;
        public override event IsEnabledHandler HudCrosshair;
        public override event IsEnabledHandler HudEnemyTag;
        public override event IsEnabledHandler HudExplosiveIcons;
        public override event IsEnabledHandler HudGameMode;
        public override event IsEnabledHandler HudHealthAmmo;
        public override event IsEnabledHandler HudMinimap;
        public override event IsEnabledHandler HudObiturary;
        public override event IsEnabledHandler HudPointsTracker;
        public override event IsEnabledHandler HudUnlocks;
        public override event PlaylistSetHandler Playlist;

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

        #region BF3 & MoHW

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

        #region MoHW only

        // VarsHudBuddyOutline
        public override void SendSetVarsBuddyOutlinePacket(bool enabled)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.buddyOutline", Packet.Bltos(enabled));
            }
        }

        public override void SendGetVarsBuddyOutlinePacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.buddyOutline");
            }
        }

        // VarsHudBuddyInfo
        public override void SendSetVarsHudBuddyInfoPacket(bool enabled)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.hudBuddyInfo", Packet.Bltos(enabled));
            }
        }

        public override void SendGetVarsHudBuddyInfoPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.hudBuddyInfo");
            }
        }

        // VarsHudClassAbility
        public override void SendSetVarsHudClassAbilityPacket(bool enabled)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.hudClassAbility", Packet.Bltos(enabled));
            }
        }

        public override void SendGetVarsHudClassAbilityPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.hudClassAbility");
            }
        }

        // VarsHudCrosshair
        public override void SendSetVarsHudCrosshairPacket(bool enabled)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.hudCrosshair", Packet.Bltos(enabled));
            }
        }

        public override void SendGetVarsHudCrosshairPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.hudCrosshair");
            }
        }

        // VarsHudEnemyTag
        public override void SendSetVarsHudEnemyTagPacket(bool enabled)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.hudEnemyTag", Packet.Bltos(enabled));
            }
        }

        public override void SendGetVarsHudEnemyTagPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.hudEnemyTag");
            }
        }

        // VarsHudExplosiveIcons
        public override void SendSetVarsHudExplosiveIconsPacket(bool enabled)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.hudExplosiveIcons", Packet.Bltos(enabled));
            }
        }

        public override void SendGetVarsHudExplosiveIconsPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.hudExplosiveIcons");
            }
        }

        // VarsHudGameMode
        public override void SendSetVarsHudGameModePacket(bool enabled)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.hudGameMode", Packet.Bltos(enabled));
            }
        }

        public override void SendGetVarsHudGameModePacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.hudGameMode");
            }
        }

        // VarsHudHealthAmmo
        public override void SendSetVarsHudHealthAmmoPacket(bool enabled)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.hudHealthAmmo", Packet.Bltos(enabled));
            }
        }

        public override void SendGetVarsHudHealthAmmoPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.hudHealthAmmo");
            }
        }

        // VarsHudMinimapResponse
        public override void SendSetVarsHudMinimapPacket(bool enabled)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.hudMinimap", Packet.Bltos(enabled));
            }
        }

        public override void SendGetVarsHudMinimapPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.hudMinimap");
            }
        }

        // VarsHudObiturary
        public override void SendSetVarsHudObituraryPacket(bool enabled)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.hudObiturary", Packet.Bltos(enabled));
            }
        }

        public override void SendGetVarsHudObituraryPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.hudObiturary");
            }
        }

        // VarsHudPointsTracker
        public override void SendSetVarsHudPointsTrackerPacket(bool enabled)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.hudPointsTracker", Packet.Bltos(enabled));
            }
        }

        public override void SendGetVarsHudPointsTrackerPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.hudPointsTracker");
            }
        }

        // VarsHudUnlocks
        public override void SendSetVarsHudUnlocksPacket(bool enabled)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.hudUnlocks", Packet.Bltos(enabled));
            }
        }

        public override void SendGetVarsHudUnlocksPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.hudUnlocks");
            }
        }

        // VarsPlaylist
        public override void SendSetVarsPlaylistPacket(string playlist)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.playlist", playlist);
            }
        }

        public override void SendGetVarsPlaylistPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.playlist");
            }
        }

        #endregion

        #endregion

        #region Level Vars (Non existent in BF3)

        public override void SendLevelVarsListPacket(LevelVariableContext context)
        {
            // Do nothing!
        }

        #endregion

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
                    "GameMod", // Note: if another variable is affixed to both games this method
                    // "Mappack", // will need to be split into MoHClient and BFBC2Client.
                    // "ExternalGameIpandPort",
                    "PunkBusterVersion",
                    "Placeholder01",
                    "ServerRegion",
                    "PingSite",
                    "ServerCountry",
                    "JoinQueueEnabled"
                }, cpRecievedPacket.Words.GetRange(1, cpRecievedPacket.Words.Count - 1));

                this.ServerInfo(this, newServerInfo);
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

        // obsolet since R-21 BF3 but back in with MoHW R-6: This can probably be removed once the onChat event is un-lamed.
        protected override void DispatchPlayerOnChatRequest(FrostbiteConnection sender, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 3)
            {
                if (GlobalChat != null)
                {
                    this.GlobalChat(this, cpRequestPacket.Words[1], cpRequestPacket.Words[2]);
                }

                cpRequestPacket.Words.RemoveAt(0);
                if (Chat != null)
                {
                    this.Chat(this, cpRequestPacket.Words);
                }
            }
        }

        #region Map Functions

        /*
        protected override void DispatchAdminRunNextRoundResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket) {
            if (cpRequestPacket.Words.Count >= 1) {
                if (this.RunNextRound != null) {
                    this.RunNextRound(this);
                }
            }
        }
        */

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

        /*
        protected override void DispatchAdminRestartRoundResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket) {
            if (cpRequestPacket.Words.Count >= 1) {
                if (this.RestartRound != null) {
                    this.RestartRound(this);
                }
            }
        }
        */

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

        /*
        protected override void DispatchMapListLoadResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket) {
            if (cpRequestPacket.Words.Count >= 1) {
                if (this.MapListLoad != null) {
                    this.MapListLoad(this);
                }
            }
        }

        protected override void DispatchMapListSaveResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket) {
            if (cpRequestPacket.Words.Count >= 1) {
                if (this.MapListSave != null) {
                    this.MapListSave(this);
                }
            }
        }
        */

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
                cpRecievedPacket.Words.RemoveAt(1); // quick hack for additional playlist info parameter (R-5)

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
                                    Match match = Regex.Match(data, @"MOHW\/Levels\/MP\/.*?\/(.*)");
                                    if (match.Success)
                                    {
                                        data = match.Groups[1].Value;
                                    }
                                    // orginal
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

                    #endregion // oldStuff
                }

                #endregion //parse mapList.List output

                if (lstMaplist.Count > 0)
                {
                    FullMapList.AddRange(lstMaplist);
                    /* quick MOHW hack mapList.list
                    this.SendMapListListRoundsPacket(iRequestStartOffset + 100);
                }
                else { */
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

                if (MapListMapAppended != null)
                {
                    if (index == -1)
                    {
                        this.MapListMapAppended(this, mapEntry);
                    }
                    else
                    {
                        this.MapListMapInserted(this, mapEntry);
                    }
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

        protected override void DispatchReservedSlotsListResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                int iRequestStartOffset = 0;

                if (cpRequestPacket.Words.Count >= 2)
                {
                    if (int.TryParse(cpRequestPacket.Words[1], out iRequestStartOffset) == false)
                    {
                        iRequestStartOffset = 0;
                    }
                }
                //
                /* if (iRequestStartOffset == 0)
                {
                    this.lstFullReservedSlotsList.Clear();
                } */
                if (iRequestStartOffset == 0)
                {
                    FullReservedSlotsList.Clear();
                    LastReservedSlotsListOffset = 0;
                }
                else
                {
                    if (LastReservedSlotsListOffset < iRequestStartOffset)
                    {
                        LastReservedSlotsListOffset = iRequestStartOffset;
                    }
                    else
                    {
                        return;
                    }
                }

                cpRecievedPacket.Words.RemoveAt(0);
                // List<String> lstReservedSlotsList = new List<String>();

                if (cpRecievedPacket.Words.Count > 0)
                {
                    FullReservedSlotsList.AddRange(cpRecievedPacket.Words);
                    /* quick MoHW hack reservedSlotsList.list
                    this.SendReservedSlotsListPacket(iRequestStartOffset + 100);
                }
                else
                { */
                    // original
                    if (ReservedSlotsList != null)
                    {
                        this.ReservedSlotsList(this, FullReservedSlotsList);
                    }
                }
            }
        }

        #endregion

        #region Vars

        #region BF3 shared

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

        #region MoHW

        /* deprecated R-5
        protected virtual void DispatchVarsAllUnlocksUnlockedResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket) {
            if (cpRequestPacket.Words.Count >= 1) {
                if (this.AllUnlocksUnlocked != null) {
                    if (cpRecievedPacket.Words.Count == 2) {
                        this.AllUnlocksUnlocked(this, Convert.ToBoolean(cpRecievedPacket.Words[1]));
                    } else if (cpRequestPacket.Words.Count >= 2) {
                        this.AllUnlocksUnlocked(this, Convert.ToBoolean(cpRequestPacket.Words[1]));
                    }
                }
            }
        }
        */

        protected virtual void DispatchVarsBuddyOutlineResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (BuddyOutline != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.BuddyOutline(this, Convert.ToBoolean(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.BuddyOutline(this, Convert.ToBoolean(cpRequestPacket.Words[1]));
                    }
                }
            }
        }

        protected virtual void DispatchVarsHudBuddyInfoResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (HudBuddyInfo != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.HudBuddyInfo(this, Convert.ToBoolean(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.HudBuddyInfo(this, Convert.ToBoolean(cpRequestPacket.Words[1]));
                    }
                }
            }
        }

        protected virtual void DispatchVarsHudClassAbilityResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (HudClassAbility != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.HudClassAbility(this, Convert.ToBoolean(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.HudClassAbility(this, Convert.ToBoolean(cpRequestPacket.Words[1]));
                    }
                }
            }
        }

        protected virtual void DispatchVarsHudCrosshairResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (HudCrosshair != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.HudCrosshair(this, Convert.ToBoolean(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.HudCrosshair(this, Convert.ToBoolean(cpRequestPacket.Words[1]));
                    }
                }
            }
        }

        protected virtual void DispatchVarsHudEnemyTagResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (HudEnemyTag != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.HudEnemyTag(this, Convert.ToBoolean(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.HudEnemyTag(this, Convert.ToBoolean(cpRequestPacket.Words[1]));
                    }
                }
            }
        }

        protected virtual void DispatchVarsHudExplosiveIconsResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (HudExplosiveIcons != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.HudExplosiveIcons(this, Convert.ToBoolean(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.HudExplosiveIcons(this, Convert.ToBoolean(cpRequestPacket.Words[1]));
                    }
                }
            }
        }

        protected virtual void DispatchVarsHudGameModeResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (HudGameMode != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.HudGameMode(this, Convert.ToBoolean(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.HudGameMode(this, Convert.ToBoolean(cpRequestPacket.Words[1]));
                    }
                }
            }
        }

        protected virtual void DispatchVarsHudHealthAmmoResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (HudHealthAmmo != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.HudHealthAmmo(this, Convert.ToBoolean(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.HudHealthAmmo(this, Convert.ToBoolean(cpRequestPacket.Words[1]));
                    }
                }
            }
        }

        protected virtual void DispatchVarsHudMinimapResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (HudMinimap != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.HudMinimap(this, Convert.ToBoolean(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.HudMinimap(this, Convert.ToBoolean(cpRequestPacket.Words[1]));
                    }
                }
            }
        }

        protected virtual void DispatchVarsHudObituraryResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (HudObiturary != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.HudObiturary(this, Convert.ToBoolean(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.HudObiturary(this, Convert.ToBoolean(cpRequestPacket.Words[1]));
                    }
                }
            }
        }

        protected virtual void DispatchVarsHudPointsTrackerResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (HudPointsTracker != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.HudPointsTracker(this, Convert.ToBoolean(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.HudPointsTracker(this, Convert.ToBoolean(cpRequestPacket.Words[1]));
                    }
                }
            }
        }

        protected virtual void DispatchVarsHudUnlocksResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (HudUnlocks != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.HudUnlocks(this, Convert.ToBoolean(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.HudUnlocks(this, Convert.ToBoolean(cpRequestPacket.Words[1]));
                    }
                }
            }
        }

        protected virtual void DispatchVarsPlaylistResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (Playlist != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.Playlist(this, Convert.ToString(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.Playlist(this, Convert.ToString(cpRequestPacket.Words[1]));
                    }
                }
            }
        }

        #endregion

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

        #endregion
    }
}