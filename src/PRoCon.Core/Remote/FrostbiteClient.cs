// Copyright 2010 Geoffrey 'Phogue' Green
// 
// http://www.phogue.net
//  
// This file is part of PRoCon Frostbite.
// 
// PRoCon Frostbite is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// PRoCon Frostbite is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with PRoCon Frostbite.  If not, see <http://www.gnu.org/licenses/>.

using PRoCon.Core.Maps;
using PRoCon.Core.Players;
using PRoCon.Core.TextChatModeration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace PRoCon.Core.Remote
{
    public class FrostbiteClient
    {
        public FrostbiteClient(FrostbiteConnection connection)
        {
            Connection = connection;
            Connection.PacketReceived += new FrostbiteConnection.PacketDispatchHandler(Connection_PacketRecieved);
            Connection.PacketCacheIntercept += new FrostbiteConnection.PacketCacheDispatchHandler(Connection_PacketCacheIntercept);
            // Register.

            Login += new EmptyParamterHandler(FrostbiteClient_Login);
            Version += new VersionHandler(FrostbiteClient_Version);

            VersionNumberToFriendlyName = new Dictionary<string, string>();

            ResponseDelegates = new Dictionary<string, ResponsePacketHandler>() {
                #region Global/Login

                {"login.plainText", DispatchLoginPlainTextResponse},
                {"login.hashed", DispatchLoginHashedResponse},
                {"logout", DispatchLogoutResponse},
                {"quit", DispatchQuitResponse},
                {"version", DispatchVersionResponse},
                {"eventsEnabled", DispatchEventsEnabledResponse},
                {"help", DispatchHelpResponse},
                {"admin.runScript", DispatchAdminRunScriptResponse},
                {"punkBuster.pb_sv_command", DispatchPunkbusterPbSvCommandResponse},
                {"serverInfo", DispatchServerInfoResponse},
                {"admin.say", DispatchAdminSayResponse},
                {"admin.yell", DispatchAdminYellResponse},

                #endregion

                #region Map list functions

                {"admin.restartMap", DispatchAdminRestartRoundResponse},
                {"admin.supportedMaps", DispatchAdminSupportedMapsResponse},
                {"admin.getPlaylists", DispatchAdminGetPlaylistsResponse},
                {"admin.listPlayers", DispatchAdminListPlayersResponse},
                {"admin.endRound", DispatchAdminEndRoundResponse},
                {"admin.runNextRound", DispatchAdminRunNextRoundResponse},
                {"admin.restartRound", DispatchAdminRestartRoundResponse},

                #endregion

                #region Banlist

                {"banList.add", DispatchBanListAddResponse},
                {"banList.remove", DispatchBanListRemoveResponse},
                {"banList.clear", DispatchBanListClearResponse},
                {"banList.save", DispatchBanListSaveResponse},
                {"banList.load", DispatchBanListLoadResponse},
                {"banList.list", DispatchBanListListResponse},

                #endregion

                #region Text Chat Moderation

                {"textChatModerationList.addPlayer", DispatchTextChatModerationAddPlayerResponse},
                {"textChatModerationList.removePlayer", DispatchTextChatModerationListRemovePlayerResponse},
                {"textChatModerationList.clear", DispatchTextChatModerationListClearResponse},
                {"textChatModerationList.save", DispatchTextChatModerationListSaveResponse},
                {"textChatModerationList.load", DispatchTextChatModerationListLoadResponse},
                {"textChatModerationList.list", DispatchTextChatModerationListListResponse},

                #endregion

                {"vars.textChatModerationMode", DispatchVarsTextChatModerationModeResponse},
                {"vars.textChatSpamTriggerCount", DispatchVarsTextChatSpamTriggerCountResponse},
                {"vars.textChatSpamDetectionTime", DispatchVarsTextChatSpamDetectionTimeResponse},
                {"vars.textChatSpamCoolDownTime", DispatchVarsTextChatSpamCoolDownTimeResponse},

                #region Maplist

                {"mapList.configFile", DispatchMapListConfigFileResponse},
                {"mapList.load", DispatchMapListLoadResponse},
                {"mapList.save", DispatchMapListSaveResponse},
                {"mapList.list", DispatchMapListListResponse},
                {"mapList.clear", DispatchMapListClearResponse},
                {"mapList.append", DispatchMapListAppendResponse},
                {"mapList.nextLevelIndex", DispatchMapListNextLevelIndexResponse},
                {"mapList.remove", DispatchMapListRemoveResponse},
                {"mapList.insert", DispatchMapListInsertResponse},

                #endregion

                // Details
                {"vars.serverName", DispatchVarsServerNameResponse},
                {"vars.serverDescription", DispatchVarsServerDescriptionResponse},
                {"vars.bannerUrl", DispatchVarsBannerUrlResponse},

                // Configuration
                {"vars.adminPassword", DispatchVarsAdminPasswordResponse},
                {"vars.gamePassword", DispatchVarsGamePasswordResponse},
                {"vars.punkBuster", DispatchVarsPunkbusterResponse},
                {"vars.ranked", DispatchVarsRankedResponse},
                {"vars.playerLimit", DispatchVarsPlayerLimitResponse},
                {"vars.currentPlayerLimit", DispatchVarsCurrentPlayerLimitResponse},
                {"vars.maxPlayerLimit", DispatchVarsMaxPlayerLimitResponse},
                {"vars.idleTimeout", DispatchVarsIdleTimeoutResponse},
                {"vars.profanityFilter", DispatchVarsProfanityFilterResponse},
                
                // Gameplay
                {"vars.friendlyFire", DispatchVarsFriendlyFireResponse},
                {"vars.hardCore", DispatchVarsHardCoreResponse},

                #region Team killing

                {"vars.teamKillCountForKick", DispatchVarsTeamKillCountForKickResponse},
                {"vars.teamKillValueForKick", DispatchVarsTeamKillValueForKickResponse},
                {"vars.teamKillValueIncrease", DispatchVarsTeamKillValueIncreaseResponse},
                {"vars.teamKillValueDecreasePerSecond", DispatchVarsTeamKillValueDecreasePerSecondResponse},

                #endregion

                #region Level vars

                {"levelVars.set", DispatchLevelVarsSetResponse},
                {"levelVars.get", DispatchLevelVarsGetResponse},
                {"levelVars.evaluate", DispatchLevelVarsEvaluateResponse},
                {"levelVars.clear", DispatchLevelVarsClearResponse},
                {"levelVars.list", DispatchLevelVarsListResponse},

                #endregion

                {"admin.kickPlayer", DispatchAdminKickPlayerResponse},
                {"admin.killPlayer", DispatchAdminKillPlayerResponse},
                {"admin.movePlayer", DispatchAdminMovePlayerResponse},
                {"admin.shutDown", DispatchAdminShutDownResponse},
            };

            RequestDelegates = new Dictionary<string, RequestPacketHandler>() {
                {"player.onJoin", DispatchPlayerOnJoinRequest},
                {"player.onLeave", DispatchPlayerOnLeaveRequest},
                {"player.onDisconnect", DispatchPlayerOnDisconnectRequest},
                {"player.onAuthenticated", DispatchPlayerOnAuthenticatedRequest},
                {"player.onKill", DispatchPlayerOnKillRequest},
                {"player.onChat", DispatchPlayerOnChatRequest},
                {"player.onKicked", DispatchPlayerOnKickedRequest},
                {"player.onTeamChange", DispatchPlayerOnTeamChangeRequest},
                {"player.onSquadChange", DispatchPlayerOnSquadChangeRequest},
                {"player.onSpawn", DispatchPlayerOnSpawnRequest},
                {"server.onLoadingLevel", DispatchServerOnLoadingLevelRequest},
                {"server.onLevelStarted", DispatchServerOnLevelStartedRequest},
                {"server.onLevelLoaded", DispatchServerOnLevelLoadedRequest},
                {"server.onRoundOver", DispatchServerOnRoundOverRequest},
                {"server.onRoundOverPlayers", DispatchServerOnRoundOverPlayersRequest},
                {"server.onRoundOverTeamScores", DispatchServerOnRoundOverTeamScoresRequest},
                {"punkBuster.onMessage", DispatchPunkBusterOnMessageRequest},
            };

            GetPacketsPattern = new Regex(@"^punkBuster\.pb_sv_command|^version|^help|^serverInfo|^admin\.listPlayers|^listPlayers|^admin\.supportMaps|^admin\.getPlaylists|^admin\.currentLevel|^mapList\.nextLevelIndex|^mapList\.list|^textChatModerationList\.list|^banList\.list|^levelVars\.list|^levelVars\.evaluate|^levelVars\.get|^vars\.[a-zA-Z]*?$");
        }

        #region Initial List

        public virtual void FetchStartupVariables()
        {
            SendServerinfoPacket();
            SendVersionPacket();
            SendHelpPacket();

            // Lists
            SendAdminListPlayersPacket(new CPlayerSubset(CPlayerSubset.PlayerSubsetType.All));
            SendReservedSlotsListPacket();
            SendMapListListRoundsPacket();

            SendBanListListPacket();

            // Vars
            // Vars - Details
            SendGetVarsGamePasswordPacket();
            //this.SendGetVarsAdminPasswordPacket();


            SendGetVarsServerNamePacket();
            SendGetVarsServerDescriptionPacket();

            // Vars - Gameplay
            SendGetVarsFriendlyFirePacket();

            // Vars - Configuration
            SendGetVarsIdleTimeoutPacket();

            // Team Kill settings
            SendGetVarsTeamKillCountForKickPacket();
            SendGetVarsTeamKillValueForKickPacket();
            SendGetVarsTeamKillValueIncreasePacket();
            SendGetVarsTeamKillValueDecreasePerSecondPacket();


            //this.SendRequest(new List<string>() { "punkBuster.pb_sv_command", "pb_sv_plist" });
        }

        #endregion

        #region Login

        public static string GeneratePasswordHash(byte[] salt, string strData)
        {
            MD5 md5Hasher = MD5.Create();

            var combined = new byte[salt.Length + strData.Length];
            salt.CopyTo(combined, 0);
            Encoding.Default.GetBytes(strData).CopyTo(combined, salt.Length);

            byte[] hash = md5Hasher.ComputeHash(combined);

            var sbStringifyHash = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sbStringifyHash.Append(hash[i].ToString("X2"));
            }

            return sbStringifyHash.ToString();
        }

        protected byte[] HashToByteArray(string strHexString)
        {
            var bytes = new byte[strHexString.Length / 2];

            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(strHexString.Substring(i * 2, 2), 16);
            }

            return bytes;
        }

        public virtual void SendLoginHashedPacket(string password)
        {
            Password = password;

            Connection.SendQueued(new Packet(false, false, Connection.AcquireSequenceNumber, new List<string>() {
                "login.hashed"
            }));
        }

        private void FrostbiteClient_Login(object sender)
        {
            SendEventsEnabledPacket(true);
        }

        #endregion

        #region Packet Helpers

        protected void BuildSendPacket(params string[] words)
        {
            Connection.SendQueued(new Packet(false, false, Connection.AcquireSequenceNumber, new List<string>(words)));
        }

        public virtual void SendEventsEnabledPacket(bool isEventsEnabled)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("eventsEnabled", Packet.Bltos(isEventsEnabled));
            }
        }

        public virtual void SendAdminSayPacket(string text, CPlayerSubset subset)
        {
            if (IsLoggedIn == true)
            {
                if (subset.Subset == CPlayerSubset.PlayerSubsetType.All)
                {
                    BuildSendPacket("admin.say", text, "all");
                }
                else if (subset.Subset == CPlayerSubset.PlayerSubsetType.Team)
                {
                    BuildSendPacket("admin.say", text, "team", subset.TeamID.ToString(CultureInfo.InvariantCulture));
                }
                else if (subset.Subset == CPlayerSubset.PlayerSubsetType.Squad)
                {
                    BuildSendPacket("admin.say", text, "squad", subset.TeamID.ToString(CultureInfo.InvariantCulture), subset.SquadID.ToString(CultureInfo.InvariantCulture));
                }
                else if (subset.Subset == CPlayerSubset.PlayerSubsetType.Player)
                {
                    BuildSendPacket("admin.say", text, "player", subset.SoldierName);
                }
            }
        }

        public virtual void SendAdminMovePlayerPacket(string soldierName, int destinationTeamId, int destinationSquadId, bool forceKill)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("admin.movePlayer", soldierName, destinationTeamId.ToString(CultureInfo.InvariantCulture), destinationSquadId.ToString(CultureInfo.InvariantCulture), Packet.Bltos(forceKill));
            }
        }

        #region General

        public virtual void SendServerinfoPacket()
        {
            BuildSendPacket("serverInfo");
        }

        public virtual void SendVersionPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("version");
            }
        }

        public virtual void SendHelpPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("help");
            }
        }

        public virtual void SendAdminListPlayersPacket(CPlayerSubset subset)
        {
            if (IsLoggedIn == true)
            {
                if (subset.Subset == CPlayerSubset.PlayerSubsetType.All)
                {
                    BuildSendPacket("admin.listPlayers", "all");
                }
                else if (subset.Subset == CPlayerSubset.PlayerSubsetType.Player)
                {
                    BuildSendPacket("admin.listPlayers", "player", subset.SoldierName);
                }
                else if (subset.Subset == CPlayerSubset.PlayerSubsetType.Team)
                {
                    BuildSendPacket("admin.listPlayers", "team", subset.TeamID.ToString(CultureInfo.InvariantCulture));
                }
                else if (subset.Subset == CPlayerSubset.PlayerSubsetType.Squad)
                {
                    BuildSendPacket("admin.listPlayers", "squad", subset.TeamID.ToString(CultureInfo.InvariantCulture), subset.SquadID.ToString(CultureInfo.InvariantCulture));
                }
            }
        }

        #endregion

        #region Map Controls

        public virtual void SendAdminRestartRoundPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("admin.restartRound");
            }
        }

        public virtual void SendAdminRunNextRoundPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("admin.runNextRound");
            }
        }

        public virtual void SendGetMaplistGetMapIndicesPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("mapList.getMapIndices");
            }
        }

        public virtual void SendGetMaplistNextLevelIndexPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("mapList.nextLevelIndex");
            }
        }

        #endregion

        #region Maplist

        public virtual void SendMapListListRoundsPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("mapList.list", "rounds");
            }
        }

        public virtual void SendMapListClearPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("mapList.clear");
            }
        }

        public virtual void SendMapListSavePacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("mapList.save");
            }
        }

        public virtual void SendMapListLoadPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("mapList.load");
            }
        }

        public virtual void SendMapListAppendPacket(MaplistEntry map)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("mapList.append", map.MapFileName, map.Rounds.ToString(CultureInfo.InvariantCulture));
            }
        }

        public virtual void SendMapListRemovePacket(int index)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("mapList.remove", index.ToString(CultureInfo.InvariantCulture));
            }
        }

        public virtual void SendMapListInsertPacket(MaplistEntry map)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("mapList.insert", map.Index.ToString(CultureInfo.InvariantCulture), map.MapFileName, map.Rounds.ToString(CultureInfo.InvariantCulture));
            }
        }

        public virtual void SendMapListNextLevelIndexPacket(int index)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("mapList.nextLevelIndex", index.ToString(CultureInfo.InvariantCulture));
            }
        }

        public virtual void SendAdminSetPlaylistPacket(string playList)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("admin.setPlaylist", playList);
            }
        }

        #endregion

        #region Ban List

        public virtual void SendBanListSavePacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("banList.save");
            }
        }

        public virtual void SendBanListListPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("banList.list");
            }
        }

        public virtual void SendBanListClearPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("banList.clear");
            }
        }

        public virtual void SendBanListListPacket(int startIndex)
        {
            if (IsLoggedIn == true)
            {
                if (startIndex >= 0)
                {
                    BuildSendPacket("banList.list", startIndex.ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    BuildSendPacket("banList.list");
                }
            }
        }

        #endregion

        #region Text Moderation List

        public virtual void SendTextChatModerationListListPacket()
        {
            SendTextChatModerationListListPacket(0);
        }

        public virtual void SendTextChatModerationListListPacket(int startIndex)
        {
            if (IsLoggedIn == true)
            {
                if (startIndex >= 0)
                {
                    BuildSendPacket("textChatModerationList.list", startIndex.ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    BuildSendPacket("textChatModerationList.list");
                }
            }
        }

        public virtual void SendTextChatModerationListAddPacket(TextChatModerationEntry playerEntry)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("textChatModerationList.addPlayer", playerEntry.PlayerModerationLevel.ToString().ToLower(), playerEntry.SoldierName);
            }
        }

        public virtual void SendTextChatModerationListRemovePacket(string soldierName)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("textChatModerationList.removePlayer", soldierName);
            }
        }

        public virtual void SendTextChatModerationListSavePacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("textChatModerationList.save");
            }
        }

        #endregion

        #region Level Variables

        public virtual void SendLevelVarsSetPacket(LevelVariableContext context, string variable, string value)
        {
            if (IsLoggedIn == true)
            {
                if (context.ContextTarget.Length > 0)
                {
                    BuildSendPacket("levelVars.set", context.ContextType.ToString().ToLower(), context.ContextTarget, variable, value);
                }
                else
                {
                    BuildSendPacket("levelVars.set", context.ContextType.ToString().ToLower(), variable, value);
                }
            }
        }

        public virtual void SendLevelVarsListPacket(LevelVariableContext context)
        {
            if (IsLoggedIn == true)
            {
                if (context.ContextTarget.Length > 0)
                {
                    BuildSendPacket("levelVars.list", context.ContextType.ToString().ToLower(), context.ContextTarget);
                }
                else
                {
                    BuildSendPacket("levelVars.list", context.ContextType.ToString().ToLower());
                }
            }
        }

        public virtual void SendLevelVarsClearPacket(LevelVariableContext context)
        {
            if (IsLoggedIn == true)
            {
                if (context.ContextTarget.Length > 0)
                {
                    BuildSendPacket("levelVars.clear", context.ContextType.ToString().ToLower(), context.ContextTarget);
                }
                else
                {
                    BuildSendPacket("levelVars.clear", context.ContextType.ToString().ToLower());
                }
            }
        }

        #endregion

        #region Reserved Slot List

        public virtual void SendReservedSlotsLoadPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("reservedSlots.load");
            }
        }

        public virtual void SendReservedSlotsListPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("reservedSlots.list");
            }
        }

        public virtual void SendReservedSlotsAddPlayerPacket(string soldierName)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("reservedSlots.addPlayer", soldierName);
            }
        }

        public virtual void SendReservedSlotsRemovePlayerPacket(string soldierName)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("reservedSlots.removePlayer", soldierName);
            }
        }

        public virtual void SendReservedSlotsSavePacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("reservedSlots.save");
            }
        }

        #endregion

        #region Spectator List

        public virtual void SendSpectatorListLoadPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("spectatorList.load");
            }
        }

        public virtual void SendSpectatorListListPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("spectatorList.list");
            }
        }

        public virtual void SendSpectatorListAddPlayerPacket(string soldierName)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("spectatorList.add", soldierName);
            }
        }

        public virtual void SendSpectatorListRemovePlayerPacket(string soldierName)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("spectatorList.remove", soldierName);
            }
        }

        public virtual void SendSpectatorListSavePacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("spectatorList.save");
            }
        }

        #endregion


        #region Game Admin List

        public virtual void SendGameAdminLoadPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("gameAdmin.load");
            }
        }

        public virtual void SendGameAdminListPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("gameAdmin.list");
            }
        }

        public virtual void SendGameAdminAddPlayerPacket(string soldierName)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("gameAdmin.add", soldierName);
            }
        }

        public virtual void SendGameAdminRemovePlayerPacket(string soldierName)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("gameAdmin.remove", soldierName);
            }
        }

        public virtual void SendGameAdminSavePacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("gameAdmin.save");
            }
        }

        #endregion
        #region Unlock List

        // This is mostly BF3 specific.

        public virtual void SendUnlockListLoadPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("unlockList.load");
            }
        }

        public virtual void SendUnlockListListPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("unlockList.list");
            }
        }

        public virtual void SendUnlockListAddPacket(string unlock)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("unlockList.add", unlock);
            }
        }

        public virtual void SendUnlockListRemovePacket(string unlock)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("unlockList.remove", unlock);
            }
        }

        public virtual void SendUnlockListSavePacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("unlockList.save");
            }
        }

        #endregion

        #region Vars

        public virtual void SendAdminSupportedMapsPacket(string playList)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("admin.supportedMaps", playList);
            }
        }

        public virtual void SendSetVarsAdminPasswordPacket(string password)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.adminPassword", password);
            }
        }

        public virtual void SendGetVarsAdminPasswordPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.adminPassword");
            }
        }

        public virtual void SendSetVarsGamePasswordPacket(string password)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.gamePassword", password);
            }
        }

        public virtual void SendGetVarsGamePasswordPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.gamePassword");
            }
        }

        public virtual void SendSetVarsPunkBusterPacket(bool enabled)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.punkBuster", Packet.Bltos(enabled));
            }
        }

        public virtual void SendGetVarsPunkBusterPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.punkBuster");
            }
        }

        public virtual void SendSetVarsHardCorePacket(bool enabled)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.hardCore", Packet.Bltos(enabled));
            }
        }

        public virtual void SendGetVarsHardCorePacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.hardCore");
            }
        }

        public virtual void SendSetVarsRankedPacket(bool enabled)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.ranked", Packet.Bltos(enabled));
            }
        }

        public virtual void SendGetVarsRankedPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.ranked");
            }
        }

        public virtual void SendSetVarsFriendlyFirePacket(bool enabled)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.friendlyFire", Packet.Bltos(enabled));
            }
        }

        public virtual void SendGetVarsFriendlyFirePacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.friendlyFire");
            }
        }

        public virtual void SendSetVarsPlayerLimitPacket(int limit)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.playerLimit", limit.ToString(CultureInfo.InvariantCulture));
            }
        }

        public virtual void SendGetVarsPlayerLimitPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.playerLimit");
            }
        }

        public virtual void SendGetVarsCurrentPlayerLimitPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.currentPlayerLimit");
            }
        }

        public virtual void SendGetVarsMaxPlayerLimitPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.maxPlayerLimit");
            }
        }

        public virtual void SendSetVarsBannerUrlPacket(string url)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.bannerUrl", url);
            }
        }

        public virtual void SendGetVarsBannerUrlPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.bannerUrl");
            }
        }

        public virtual void SendSetVarsServerDescriptionPacket(string description)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.serverDescription", description);
            }
        }

        public virtual void SendGetVarsServerDescriptionPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.serverDescription");
            }
        }

        public virtual void SendSetVarsServerNamePacket(string serverName)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.serverName", serverName);
            }
        }

        public virtual void SendGetVarsServerNamePacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.serverName");
            }
        }

        public virtual void SendSetVarsIdleTimeoutPacket(int limit)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.idleTimeout", limit.ToString(CultureInfo.InvariantCulture));
            }
        }

        public virtual void SendGetVarsIdleTimeoutPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.idleTimeout");
            }
        }

        public virtual void SendSetVarsIdleBanRoundsPacket(int limit)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.idleBanRounds", limit.ToString(CultureInfo.InvariantCulture));
            }
        }

        public virtual void SendGetVarsIdleBanRoundsPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.idleBanRounds");
            }
        }

        public virtual void SendSetVarsProfanityFilterPacket(bool enabled)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.profanityFilter", Packet.Bltos(enabled));
            }
        }

        public virtual void SendGetVarsProfanityFilterPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.profanityFilter");
            }
        }

        public virtual void SendSetVarsTeamKillCountForKickPacket(int limit)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.teamKillCountForKick", limit.ToString(CultureInfo.InvariantCulture));
            }
        }

        public virtual void SendGetVarsTeamKillCountForKickPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.teamKillCountForKick");
            }
        }

        public virtual void SendSetVarsTeamKillKickForBanPacket(int limit)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.teamKillKickForBan", limit.ToString(CultureInfo.InvariantCulture));
            }
        }

        public virtual void SendGetVarsTeamKillKickForBanPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.teamKillKickForBan");
            }
        }

        public virtual void SendSetVarsTeamKillValueForKickPacket(int limit)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.teamKillValueForKick", limit.ToString(CultureInfo.InvariantCulture));
            }
        }

        public virtual void SendGetVarsTeamKillValueForKickPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.teamKillValueForKick");
            }
        }

        public virtual void SendSetVarsTeamKillValueIncreasePacket(int limit)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.teamKillValueIncrease", limit.ToString(CultureInfo.InvariantCulture));
            }
        }

        public virtual void SendGetVarsTeamKillValueIncreasePacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.teamKillValueIncrease");
            }
        }

        public virtual void SendSetVarsTeamKillValueDecreasePerSecondPacket(int limit)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.teamKillValueDecreasePerSecond", limit.ToString(CultureInfo.InvariantCulture));
            }
        }

        public virtual void SendGetVarsTeamKillValueDecreasePerSecondPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.teamKillValueDecreasePerSecond");
            }
        }

        public virtual void SendAdminGetPlaylistPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("admin.getPlaylist");
            }
        }

        public virtual void SendAdminGetPlaylistsPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("admin.getPlaylists");
            }
        }

        #region Text Chat Moderation

        public virtual void SendSetVarsTextChatModerationModePacket(ServerModerationModeType mode)
        {
            if (IsLoggedIn == true && mode != ServerModerationModeType.None)
            {
                BuildSendPacket("vars.textChatModerationMode", mode.ToString().ToLower());
            }
        }

        public virtual void SendGetVarsTextChatModerationModePacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.textChatModerationMode");
            }
        }

        public virtual void SendSetVarsTextChatSpamTriggerCountPacket(int limit)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.textChatSpamTriggerCount", limit.ToString(CultureInfo.InvariantCulture));
            }
        }

        public virtual void SendGetVarsTextChatSpamTriggerCountPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.textChatSpamTriggerCount");
            }
        }

        public virtual void SendSetVarsTextChatSpamDetectionTimePacket(int limit)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.textChatSpamDetectionTime", limit.ToString(CultureInfo.InvariantCulture));
            }
        }

        public virtual void SendGetVarsTextChatSpamDetectionTimePacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.textChatSpamDetectionTime");
            }
        }

        public virtual void SendSetVarsTextChatSpamCoolDownTimePacket(int limit)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.textChatSpamCoolDownTime", limit.ToString(CultureInfo.InvariantCulture));
            }
        }

        public virtual void SendGetVarsTextChatSpamCoolDownTimePacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.textChatSpamCoolDownTime");
            }
        }

        #endregion

        #region Game Specific

        #region BFBC2

        public virtual void SendSetVarsRankLimitPacket(int limit)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.rankLimit", limit.ToString(CultureInfo.InvariantCulture));
            }
        }

        public virtual void SendGetVarsRankLimitPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.rankLimit");
            }
        }

        public virtual void SendSetVarsTeamBalancePacket(bool enabled)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.teamBalance", Packet.Bltos(enabled));
            }
        }


        public virtual void SendGetVarsTeamBalancePacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.teamBalance");
            }
        }

        public virtual void SendSetVarsKillCamPacket(bool enabled)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.killCam", Packet.Bltos(enabled));
            }
        }

        public virtual void SendGetVarsKillCamPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.killCam");
            }
        }

        public virtual void SendSetVarsMiniMapPacket(bool enabled)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.miniMap", Packet.Bltos(enabled));
            }
        }

        public virtual void SendGetVarsMiniMapPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.miniMap");
            }
        }

        public virtual void SendSetVarsCrossHairPacket(bool enabled)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.crossHair", Packet.Bltos(enabled));
            }
        }

        public virtual void SendGetVarsCrossHairPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.crossHair");
            }
        }

        // ReSharper disable InconsistentNaming
        public virtual void SendSetVars3dSpottingPacket(bool enabled)
        {

            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.3dSpotting", Packet.Bltos(enabled));
            }
        }

        public virtual void SendGetVars3dSpottingPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.3dSpotting");
            }
        }
        // ReSharper restore InconsistentNaming

        public virtual void SendSetVarsMiniMapSpottingPacket(bool enabled)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.miniMapSpotting", Packet.Bltos(enabled));
            }
        }

        public virtual void SendGetVarsMiniMapSpottingPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.miniMapSpotting");
            }
        }

        public virtual void SendSetVarsThirdPersonVehicleCamerasPacket(bool enabled)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.thirdPersonVehicleCameras", Packet.Bltos(enabled));
            }
        }

        public virtual void SendGetVarsThirdPersonVehicleCamerasPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.thirdPersonVehicleCameras");
            }
        }

        #endregion

        #region MoH

        #region Clan teams

        public virtual void SendSetVarsClanTeamsPacket(bool enabled)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.clanTeams", Packet.Bltos(enabled));
            }
        }

        public virtual void SendGetVarsClanTeamsPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.clanTeams");
            }
        }

        #endregion

        #region No Crosshairs

        public virtual void SendSetVarsNoCrosshairsPacket(bool enabled)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.noCrosshairs", Packet.Bltos(enabled));
            }
        }

        public virtual void SendGetVarsNoCrosshairsPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.noCrosshairs");
            }
        }

        #endregion

        #region Realistic Health

        public virtual void SendSetVarsRealisticHealthPacket(bool enabled)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.realisticHealth", Packet.Bltos(enabled));
            }
        }

        public virtual void SendGetVarsRealisticHealthPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.realisticHealth");
            }
        }

        #endregion

        #region No Unlocks

        public virtual void SendSetVarsNoUnlocksPacket(bool enabled)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.noUnlocks", Packet.Bltos(enabled));
            }
        }

        public virtual void SendGetVarsNoUnlocksPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.noUnlocks");
            }
        }

        #endregion

        #region No Ammo Pickups

        public virtual void SendSetVarsNoAmmoPickupsPacket(bool enabled)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.noAmmoPickups", Packet.Bltos(enabled));
            }
        }

        public virtual void SendGetVarsNoAmmoPickupsPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.noAmmoPickups");
            }
        }

        #endregion

        #region TDM Score Limit

        public virtual void SendSetVarsTdmScoreCounterMaxScorePacket(int limit)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.tdmScoreCounterMaxScore", limit.ToString(CultureInfo.InvariantCulture));
            }
        }

        public virtual void SendGetVarsTdmScoreCounterMaxScorePacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.tdmScoreCounterMaxScore");
            }
        }

        #endregion

        #region Preround Limit

        public virtual void SendSetVarsPreRoundLimitPacket(int upperLimit, int lowerLimit)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.preRoundLimit", upperLimit.ToString(CultureInfo.InvariantCulture), lowerLimit.ToString(CultureInfo.InvariantCulture));
            }
        }

        public virtual void SendGetVarsPreRoundLimitPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.preRoundLimit");
            }
        }

        #endregion

        #region Skill Limit

        public virtual void SendSetVarsSkillLimitPacket(int upperLimit, int lowerLimit)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.skillLimit", upperLimit.ToString(CultureInfo.InvariantCulture), lowerLimit.ToString(CultureInfo.InvariantCulture));
            }
        }

        public virtual void SendGetVarsSkillLimitPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.skillLimit");
            }
        }

        #endregion

        #region Preround Timer Enabled

        public virtual void SendSetAdminRoundStartTimerEnabledPacket(bool enabled)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("admin.roundStartTimerEnabled", Packet.Bltos(enabled));
            }
        }

        public virtual void SendGetAdminRoundStartTimerEnabledPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("admin.roundStartTimerEnabled");
            }
        }

        #endregion

        #region Preround Timer Delay

        public virtual void SendSetVarsRoundStartTimerDelayPacket(int limit)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.roundStartTimerDelay", limit.ToString(CultureInfo.InvariantCulture));
            }
        }

        public virtual void SendGetVarsRoundStartTimerDelayPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.roundStartTimerDelay");
            }
        }

        #endregion

        #region Preround Player Limit

        public virtual void SendSetVarsRoundStartTimerPlayersLimitPacket(int limit)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.roundStartTimerPlayersLimit", limit.ToString(CultureInfo.InvariantCulture));
            }
        }

        public virtual void SendGetVarsRoundStartTimerPlayersLimitPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.roundStartTimerPlayersLimit");
            }
        }

        #endregion

        #endregion

        #region BF3

        public virtual void SendSetVarsServerMessagePacket(string description)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.serverMessage", description);
            }
        }

        public virtual void SendGetVarsServerMessagePacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.serverMessage");
            }
        }

        public virtual void SendSetVarsVehicleSpawnAllowedPacket(bool enabled)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.vehicleSpawnAllowed", Packet.Bltos(enabled));
            }
        }

        public virtual void SendGetVarsVehicleSpawnAllowedPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.vehicleSpawnAllowed");
            }
        }


        public virtual void SendSetVarsVehicleSpawnDelayPacket(int limit)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.vehicleSpawnDelay", limit.ToString(CultureInfo.InvariantCulture));
            }
        }

        public virtual void SendGetVarsVehicleSpawnDelayPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.vehicleSpawnDelay");
            }
        }


        public virtual void SendSetVarsBulletDamagePacket(int limit)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.bulletDamage", limit.ToString(CultureInfo.InvariantCulture));
            }
        }

        public virtual void SendGetVarsBulletDamagePacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.bulletDamage");
            }
        }


        public virtual void SendSetVarsNameTagPacket(bool enabled)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.nameTag", Packet.Bltos(enabled));
            }
        }

        public virtual void SendGetVarsNameTagPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.nameTag");
            }
        }

        public virtual void SendSetVarsRegenerateHealthPacket(bool enabled)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.regenerateHealth", Packet.Bltos(enabled));
            }
        }

        public virtual void SendGetVarsRegenerateHealthPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.regenerateHealth");
            }
        }


        public virtual void SendSetVarsRoundRestartPlayerCountPacket(int limit)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.roundRestartPlayerCount", limit.ToString(CultureInfo.InvariantCulture));
            }
        }

        public virtual void SendGetVarsRoundRestartPlayerCountPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.roundRestartPlayerCount");
            }
        }


        public virtual void SendSetVarsRoundStartPlayerCountPacket(int limit)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.roundStartPlayerCount", limit.ToString(CultureInfo.InvariantCulture));
            }
        }

        public virtual void SendGetVarsRoundStartPlayerCountPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.roundStartPlayerCount");
            }
        }


        public virtual void SendSetVarsOnlySquadLeaderSpawnPacket(bool enabled)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.onlySquadLeaderSpawn", Packet.Bltos(enabled));
            }
        }

        public virtual void SendGetVarsOnlySquadLeaderSpawnPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.onlySquadLeaderSpawn");
            }
        }

        public virtual void SendSetVarsUnlockModePacket(string mode)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.unlockMode", mode.ToLower());
            }
        }

        public virtual void SendGetVarsUnlockModePacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.unlockMode");
            }
        }

        public virtual void SendSetVarsPresetPacket(string mode, bool locked)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.preset", mode, Packet.Bltos(locked));
            }
        }

        public virtual void SendGetVarsPresetPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.preset");
            }
        }

        public virtual void SendSetVarsGunMasterWeaponsPresetPacket(int preset)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.gunMasterWeaponsPreset", preset.ToString(CultureInfo.InvariantCulture));
            }
        }

        public virtual void SendGetVarsGunMasterWeaponsPresetPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.gunMasterWeaponsPreset");
            }
        }

        public virtual void SendSetVarsSoldierHealthPacket(int limit)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.soldierHealth", limit.ToString(CultureInfo.InvariantCulture));
            }
        }

        public virtual void SendGetVarsSoldierHealthPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.soldierHealth");
            }
        }


        public virtual void SendSetVarsHudPacket(bool enabled)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.hud", Packet.Bltos(enabled));
            }
        }

        public virtual void SendGetVarsHudPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.hud");
            }
        }


        public virtual void SendSetVarsPlayerManDownTimePacket(int limit)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.playerManDownTime", limit.ToString(CultureInfo.InvariantCulture));
            }
        }

        public virtual void SendGetVarsPlayerManDownTimePacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.playerManDownTime");
            }
        }

        public virtual void SendSetVarsPlayerRespawnTimePacket(int limit)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.playerRespawnTime", limit.ToString(CultureInfo.InvariantCulture));
            }
        }

        public virtual void SendGetVarsPlayerRespawnTimePacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.playerRespawnTime");
            }
        }

        public virtual void SendSetVarsGameModeCounterPacket(int limit)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.gameModeCounter", limit.ToString(CultureInfo.InvariantCulture));
            }
        }

        public virtual void SendGetVarsGameModeCounterPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.gameModeCounter");
            }
        }

        public virtual void SendSetVarsCtfRoundTimeModifierPacket(int limit)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.ctfRoundTimeModifier", limit.ToString(CultureInfo.InvariantCulture));
            }
        }

        public virtual void SendGetVarsCtfRoundTimeModifierPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.ctfRoundTimeModifier");
            }
        }


        public virtual void SendSetVarsRoundTimeLimitPacket(int limit)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.roundTimeLimit", limit.ToString(CultureInfo.InvariantCulture));
            }
        }

        public virtual void SendGetVarsRoundTimeLimitPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.roundTimeLimit");
            }
        }

        public virtual void SendSetVarsTicketBleedRatePacket(int limit)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.ticketBleedRate", limit.ToString(CultureInfo.InvariantCulture));
            }
        }

        public virtual void SendGetVarsTicketBleedRatePacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.ticketBleedRate");
            }
        }

        public virtual void SendSetReservedSlotsListAggressiveJoinPacket(bool enabled)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("reservedSlotsList.aggressiveJoin", Packet.Bltos(enabled));
            }
        }

        public virtual void SendGetReservedSlotsListAggressiveJoinPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("reservedSlotsList.aggressiveJoin");
            }
        }

        public virtual void SendSetVarsRoundLockdownCountdownPacket(int limit)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.roundLockdownCountdown", limit.ToString(CultureInfo.InvariantCulture));
            }
        }

        public virtual void SendGetVarsRoundLockdownCountdownPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.roundLockdownCountdown");
            }
        }

        public virtual void SendSetVarsRoundWarmupTimeoutPacket(int limit)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.roundWarmupTimeout", limit.ToString(CultureInfo.InvariantCulture));
            }
        }

        public virtual void SendGetVarsRoundWarmupTimeoutPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.roundWarmupTimeout");
            }
        }

        public virtual void SendSetVarsPremiumStatusPacket(bool enabled)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.premiumStatus", Packet.Bltos(enabled));
            }
        }

        public virtual void SendGetVarsPremiumStatusPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.premiumStatus");
            }
        }

        #endregion

        #region BF4

        public virtual void SendSetVarsFairFightPacket(bool enabled)
        {
            if (IsLoggedIn == true)
            {
                if (enabled == true)
                {
                    BuildSendPacket("fairFight.activate");
                }
                else
                {
                    BuildSendPacket("fairFight.deactivate");
                }
            }
        }

        public virtual void SendGetVarsFairFightPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("fairFight.isActive");
            }
        }

        public virtual void SendSetVarsMaxSpectatorsPacket(int limit)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.maxSpectators", limit.ToString(CultureInfo.InvariantCulture));
            }
        }

        public virtual void SendGetVarsMaxSpectatorsPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.maxSpectators");
            }
        }

        public virtual void SendSetVarsHitIndicatorsEnabled(bool enabled)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.hitIndicatorsEnabled", Packet.Bltos(enabled));
            }
        }

        public virtual void SendGetVarsHitIndicatorsEnabled()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.hitIndicatorsEnabled");
            }
        }

        /*
         * This value is read only.
        public virtual void SendSetVarsServerType(string value) {
            if (IsLoggedIn == true) {
                BuildSendPacket("vars.serverType", value);
            }
        }
        */
        public virtual void SendGetVarsServerType()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.serverType");
            }
        }

        public virtual void SendSetVarsCommander(bool enabled)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.commander", Packet.Bltos(enabled));
            }
        }

        public virtual void SendGetVarsCommander()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.commander");
            }
        }

        public virtual void SendSetVarsAlwaysAllowSpectators(bool enabled)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.alwaysAllowSpectators", Packet.Bltos(enabled));
            }
        }

        public virtual void SendGetVarsAlwaysAllowSpectators()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.alwaysAllowSpectators");
            }
        }

        public virtual void SendSetVarsForceReloadWholeMags(bool enabled)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.forceReloadWholeMags", Packet.Bltos(enabled));
            }
        }

        public virtual void SendGetVarsForceReloadWholeMags()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.forceReloadWholeMags");
            }
        }

        public virtual void SendSetVarsTeamFactionOverridePacket(int teamId, int faction)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.teamFactionOverride", teamId.ToString(CultureInfo.InvariantCulture), faction.ToString(CultureInfo.InvariantCulture));
            }
        }

        public virtual void SendGetVarsTeamFactionOverridePacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.teamFactionOverride");
            }
        }

        #endregion

        #region MoHW

        // VarsAllUnlocksUnlocked
        public virtual void SendSetVarsAllUnlocksUnlockedPacket(bool enabled)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.allUnlocksUnlocked", Packet.Bltos(enabled));
            }
        }

        public virtual void SendGetVarsAllUnlocksUnlockedPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.allUnlocksUnlocked");
            }
        }

        // VarsHudBuddyOutline
        public virtual void SendSetVarsBuddyOutlinePacket(bool enabled)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.buddyOutline", Packet.Bltos(enabled));
            }
        }

        public virtual void SendGetVarsBuddyOutlinePacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.buddyOutline");
            }
        }

        // VarsHudBuddyInfo
        public virtual void SendSetVarsHudBuddyInfoPacket(bool enabled)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.hudBuddyInfo", Packet.Bltos(enabled));
            }
        }

        public virtual void SendGetVarsHudBuddyInfoPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.hudBuddyInfo");
            }
        }

        // VarsHudClassAbility
        public virtual void SendSetVarsHudClassAbilityPacket(bool enabled)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.hudClassAbility", Packet.Bltos(enabled));
            }
        }

        public virtual void SendGetVarsHudClassAbilityPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.hudClassAbility");
            }
        }

        // VarsHudCrosshair
        public virtual void SendSetVarsHudCrosshairPacket(bool enabled)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.hudCrosshair", Packet.Bltos(enabled));
            }
        }

        public virtual void SendGetVarsHudCrosshairPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.hudCrosshair");
            }
        }

        // VarsHudEnemyTag
        public virtual void SendSetVarsHudEnemyTagPacket(bool enabled)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.hudEnemyTag", Packet.Bltos(enabled));
            }
        }

        public virtual void SendGetVarsHudEnemyTagPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.hudEnemyTag");
            }
        }

        // VarsHudExplosiveIcons
        public virtual void SendSetVarsHudExplosiveIconsPacket(bool enabled)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.hudExplosiveIcons", Packet.Bltos(enabled));
            }
        }

        public virtual void SendGetVarsHudExplosiveIconsPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.hudExplosiveIcons");
            }
        }

        // VarsHudGameMode
        public virtual void SendSetVarsHudGameModePacket(bool enabled)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.hudGameMode", Packet.Bltos(enabled));
            }
        }

        public virtual void SendGetVarsHudGameModePacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.hudGameMode");
            }
        }

        // VarsHudHealthAmmo
        public virtual void SendSetVarsHudHealthAmmoPacket(bool enabled)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.hudHealthAmmo", Packet.Bltos(enabled));
            }
        }

        public virtual void SendGetVarsHudHealthAmmoPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.hudHealthAmmo");
            }
        }

        // VarsHudMinimapResponse
        public virtual void SendSetVarsHudMinimapPacket(bool enabled)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.hudMinimap", Packet.Bltos(enabled));
            }
        }

        public virtual void SendGetVarsHudMinimapPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.hudMinimap");
            }
        }

        // VarsHudObiturary
        public virtual void SendSetVarsHudObituraryPacket(bool enabled)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.hudObiturary", Packet.Bltos(enabled));
            }
        }

        public virtual void SendGetVarsHudObituraryPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.hudObiturary");
            }
        }

        // VarsHudPointsTracker
        public virtual void SendSetVarsHudPointsTrackerPacket(bool enabled)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.hudPointsTracker", Packet.Bltos(enabled));
            }
        }

        public virtual void SendGetVarsHudPointsTrackerPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.hudPointsTracker");
            }
        }

        // VarsHudUnlocks
        public virtual void SendSetVarsHudUnlocksPacket(bool enabled)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.hudUnlocks", Packet.Bltos(enabled));
            }
        }

        public virtual void SendGetVarsHudUnlocksPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.hudUnlocks");
            }
        }

        // VarsPlaylist
        public virtual void SendSetVarsPlaylistPacket(string playlist)
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.playlist", playlist);
            }
        }

        public virtual void SendGetVarsPlaylistPacket()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.playlist");
            }
        }

        #endregion

        #region Battlefield: Hardline

        public virtual void SendGetVarsRoundStartReadyPlayersNeeded()
        {
            if (IsLoggedIn == true)
            {
                BuildSendPacket("vars.roundStartReadyPlayersNeeded");
            }
        }

        #endregion

        #endregion

        #endregion

        #endregion

        public FrostbiteConnection Connection { get; private set; }

        public bool IsLoggedIn { get; protected set; }

        public string Password { get; private set; }

        public virtual string GameType
        {
            get { return String.Empty; }
        }

        public string VersionNumber { get; protected set; }

        public int VersionInteger { get; protected set; }

        public Dictionary<string, string> VersionNumberToFriendlyName { get; set; }

        public string FriendlyVersionNumber
        {
            get
            {
                String versionNumber = String.Empty;

                if (VersionNumber != null && VersionNumberToFriendlyName != null && VersionNumberToFriendlyName.ContainsKey(VersionNumber) == true)
                {
                    versionNumber = VersionNumberToFriendlyName[VersionNumber];
                }
                else if (VersionNumber == null)
                {
                    versionNumber = String.Empty;
                }
                else
                {
                    versionNumber = VersionNumber;
                }

                return versionNumber;
            }
        }

        public bool IsLayered { get; set; }

        public double UtcOffset { get; set; }

        public virtual bool HasSquads
        {
            get { return true; }
        }

        public virtual bool HasOpenMaplist
        {
            get { return false; }
        }

        // Commands that simply retreive a list, but do not alter any settings
        public Regex GetPacketsPattern { get; protected set; }

        #region Delegates

        public delegate void AuthenticationFailureHandler(FrostbiteClient sender, string strError);

        public delegate void BanListAddHandler(FrostbiteClient sender, CBanInfo cbiAddedBan);

        public delegate void BanListListHandler(FrostbiteClient sender, int iStartOffset, List<CBanInfo> lstBans);

        public delegate void BanListRemoveHandler(FrostbiteClient sender, CBanInfo cbiRemovedBan);

        public delegate void BannerUrlHandler(FrostbiteClient sender, string url);

        public delegate void CurrentLevelHandler(FrostbiteClient sender, string currentLevel);

        public delegate void EmptyParamterHandler(FrostbiteClient sender);

        public delegate void EndRoundHandler(FrostbiteClient sender, int iWinningTeamID);

        public delegate void GlobalChatHandler(FrostbiteClient sender, string playerName, string message);

        public delegate void GunMasterWeaponsPresetHandler(FrostbiteClient sender, int preset);

        public delegate void HelpHandler(FrostbiteClient sender, List<string> lstCommands);

        public delegate void IsEnabledHandler(FrostbiteClient sender, bool isEnabled);

        public delegate void VarsStringHandler(FrostbiteClient sender, string value);

        public delegate void LevelLoadedHandler(FrostbiteClient sender, string mapFileName, string gamemode, int roundsPlayed, int roundsTotal);

        public delegate void LevelVariableGetHandler(FrostbiteClient sender, LevelVariable lvRequestedContext, LevelVariable lvReturnedValue);

        public delegate void LevelVariableHandler(FrostbiteClient sender, LevelVariable lvRequestedContext);

        public delegate void LevelVariableListHandler(FrostbiteClient sender, LevelVariable lvRequestedContext, List<LevelVariable> lstReturnedValues);

        public delegate void LimitHandler(FrostbiteClient sender, int limit);

        public delegate void ListPlayersHandler(FrostbiteClient sender, List<CPlayerInfo> lstPlayers, CPlayerSubset cpsSubset);

        public delegate void ListPlaylistsHandler(FrostbiteClient sender, List<string> lstPlaylists);

        public delegate void LoadingLevelHandler(FrostbiteClient sender, string mapFileName, int roundsPlayed, int roundsTotal);

        public delegate void MapListAppendedHandler(FrostbiteClient sender, MaplistEntry mapEntry);

        public delegate void MapListConfigFileHandler(FrostbiteClient sender, string strConfigFilename);

        public delegate void MapListGetMapIndicesHandler(FrostbiteClient sender, int mapIndex, int nextIndex);

        public delegate void MapListGetRoundsHandler(FrostbiteClient sender, int currentRound, int totalRounds);

        public delegate void MapListLevelIndexHandler(FrostbiteClient sender, int mapIndex);

        public delegate void MapListListedHandler(FrostbiteClient sender, List<MaplistEntry> lstMapList);

        public delegate void MapListMapInsertedHandler(FrostbiteClient sender, MaplistEntry entry);

        public delegate void PacketDispatchHandler(FrostbiteClient sender, Packet packetBeforeDispatch, bool isCommandConnection);

        public delegate void PacketDispatchedHandler(FrostbiteClient sender, Packet packetBeforeDispatch, bool isCommandConnection, out bool isProcessed);

        public delegate void PasswordHandler(FrostbiteClient sender, string password);

        public delegate void PlayerAuthenticatedHandler(FrostbiteClient sender, string playerName, string playerGuid);

        public delegate void PlayerEventHandler(FrostbiteClient sender, string playerName);

        public delegate void PlayerIdleStateHandler(FrostbiteClient sender, string soldierName, int idleTime);

        public delegate void PlayerIsAliveHandler(FrostbiteClient sender, string soldierName, bool isAlive);

        public delegate void PlayerKickedHandler(FrostbiteClient sender, string strSoldierName, string strReason);

        public delegate void PlayerKilledByAdminHandler(FrostbiteClient sender, string soldierName);

        public delegate void PlayerKilledHandler(FrostbiteClient sender, string strKiller, string strVictim, string strDamageType, bool blHeadshot, Point3D pntKiller, Point3D pntVictim);

        public delegate void PlayerLeaveHandler(FrostbiteClient sender, string playerName, CPlayerInfo cpiPlayer);

        public delegate void PlayerChatHandler(FrostbiteClient sender, string playerName, string message, string targetPlayer);

        public delegate void PlayerDisconnectedHandler(FrostbiteClient sender, string playerName, string reason);

        public delegate void PlayerMovedByAdminHandler(FrostbiteClient sender, string soldierName, int destinationTeamId, int destinationSquadId, bool forceKilled);

        public delegate void PlayerPingedByAdminHandler(FrostbiteClient sender, string soldierName, int ping);

        public delegate void PlayerSpawnedHandler(FrostbiteClient sender, string soldierName, string strKit, List<string> lstWeapons, List<string> lstSpecializations);

        public delegate void PlayerTeamChangeHandler(FrostbiteClient sender, string strSoldierName, int iTeamID, int iSquadID);

        public delegate void PlaylistSetHandler(FrostbiteClient sender, string playlist);

        public delegate void PunkbusterMessageHandler(FrostbiteClient sender, string punkbusterMessage);

        public delegate void RawChatHandler(FrostbiteClient sender, List<string> rawChat);

        public delegate void ReservedSlotsListHandler(FrostbiteClient sender, List<string> soldierNames);

        public delegate void ReservedSlotsPlayerHandler(FrostbiteClient sender, string strSoldierName);

        public delegate void ReserverdSlotsConfigFileHandler(FrostbiteClient sender, string configFilename);

        public delegate void SpectatorListListHandler(FrostbiteClient sender, List<string> soldierNames);

        public delegate void SpectatorListPlayerHandler(FrostbiteClient sender, string soldierName);

        public delegate void SpectatorListConfigFileHandler(FrostbiteClient sender, string configFilename);

        public delegate void GameAdminListHandler(FrostbiteClient sender, List<string> soldierNames);

        public delegate void GameAdminPlayerHandler(FrostbiteClient sender, string soldierName);

        public delegate void GameAdminConfigFileHandler(FrostbiteClient sender, string configFilename);

        public delegate void ResponseErrorHandler(FrostbiteClient sender, Packet originalRequest, string errorMessage);

        public delegate void RoundOverHandler(FrostbiteClient sender, int iWinningTeamID);

        public delegate void RoundOverPlayersHandler(FrostbiteClient sender, List<CPlayerInfo> lstPlayers);

        public delegate void RoundOverTeamScoresHandler(FrostbiteClient sender, List<TeamScore> lstTeamScores);

        public delegate void RunScriptErrorHandler(FrostbiteClient sender, string strScriptFileName, int iLineError, string strErrorDescription);

        public delegate void RunScriptHandler(FrostbiteClient sender, string scriptFileName);

        public delegate void SayingHandler(FrostbiteClient sender, string strMessage, List<string> lstSubsetWords);

        public delegate void SendPunkBusterMessageHandler(FrostbiteClient sender, string punkbusterMessage);

        public delegate void ServerDescriptionHandler(FrostbiteClient sender, string serverDescription);

        public delegate void ServerInfoHandler(FrostbiteClient sender, CServerInfo csiServerInfo);

        public delegate void ServerMessageHandler(FrostbiteClient sender, string serverMessage);

        public delegate void ServerNameHandler(FrostbiteClient sender, string strServerName);

        public delegate void SquadChatHandler(FrostbiteClient sender, string playerName, string message, int teamId, int squadId);

        public delegate void SquadIsPrivateHandler(FrostbiteClient sender, int teamId, int squadId, bool isPrivate);

        public delegate void SquadLeaderHandler(FrostbiteClient sender, int teamId, int squadId, string soldierName);

        public delegate void SquadListActiveHandler(FrostbiteClient sender, int teamId, int squadCount, List<int> squadList);

        public delegate void SquadListPlayersHandler(FrostbiteClient sender, int teamId, int squadId, int playerCount, List<string> playersInSquad);

        public delegate void SupportedMapsHandler(FrostbiteClient sender, string strPlaylist, List<string> lstSupportedMaps);

        public delegate void TeamChatHandler(FrostbiteClient sender, string playerName, string message, int teamId);

        public delegate void TeamFactionOverrideHandler(FrostbiteClient sender, int teamId, int faction);

        public delegate void TextChatModerationListAddPlayerHandler(FrostbiteClient sender, TextChatModerationEntry playerEntry);

        public delegate void TextChatModerationListListHandler(FrostbiteClient sender, int startOffset, List<TextChatModerationEntry> textChatModerationList);

        public delegate void TextChatModerationListRemovePlayerHandler(FrostbiteClient sender, TextChatModerationEntry playerEntry);

        public delegate void TextChatModerationModeHandler(FrostbiteClient sender, ServerModerationModeType mode);

        public delegate void UnlockModeHandler(FrostbiteClient sender, string mode);

        public delegate void BF4presetHandler(FrostbiteClient sender, string mode, bool isLocked);

        public delegate void MpExperienceHandler(FrostbiteClient sender, string mpExperience);

        public delegate void UpperLowerLimitHandler(FrostbiteClient sender, int upperLimit, int lowerLimit);

        public delegate void VersionHandler(FrostbiteClient sender, string serverType, string serverVersion);

        public delegate void YellingHandler(FrostbiteClient sender, string strMessage, int iMessageDuration, List<string> lstSubsetWords);

        #endregion

        #region Events

        public event EmptyParamterHandler Login;
        public event AuthenticationFailureHandler LoginFailure;
        public event EmptyParamterHandler Logout;
        public event EmptyParamterHandler Quit;

        public event VersionHandler Version;
        public event HelpHandler Help;

        public event IsEnabledHandler EventsEnabled;

        #endregion

        #region Packet Management

        protected Dictionary<string, RequestPacketHandler> RequestDelegates;
        protected Dictionary<string, ResponsePacketHandler> ResponseDelegates;

        protected delegate void RequestPacketHandler(FrostbiteConnection sender, Packet cpRequestPacket);

        protected delegate void ResponsePacketHandler(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket);

        #endregion

        private void FrostbiteClient_Version(FrostbiteClient sender, string serverType, string serverVersion)
        {
            VersionNumber = serverVersion;

            int versionInteger = 0;
            if (int.TryParse(serverVersion, out versionInteger) == true)
            {
                VersionInteger = versionInteger;
            }
        }

        private void Connection_PacketCacheIntercept(FrostbiteConnection sender, Packet request, Packet response)
        {
            if (request.OriginatedFromServer == false && response.IsResponse == true)
            {
                DispatchResponsePacket(sender, response, request);
            }
        }

        private void Connection_PacketRecieved(object sender, bool isHandled, Packet packetBeforeDispatch)
        {
            if (isHandled == false)
            {
                if (packetBeforeDispatch.OriginatedFromServer == false && packetBeforeDispatch.IsResponse == true)
                {
                    Packet requestPacket = Connection.GetRequestPacket(packetBeforeDispatch);

                    if (requestPacket != null)
                    {
                        DispatchResponsePacket((FrostbiteConnection)sender, (Packet)packetBeforeDispatch.Clone(), requestPacket);
                    }
                }
                else if (packetBeforeDispatch.OriginatedFromServer == true && packetBeforeDispatch.IsResponse == false)
                {
                    DispatchRequestPacket((FrostbiteConnection)sender, (Packet)packetBeforeDispatch.Clone());
                }
            }
        }

        public virtual void Shutdown()
        {
            if (Connection != null)
                Connection.Shutdown();
        }

        #region Response Handlers

        #region Basic Universal Commands

        protected virtual void DispatchLoginPlainTextResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                IsLoggedIn = true;
                if (Login != null)
                {
                    this.Login(this);
                }
            }
        }

        protected virtual void DispatchLoginHashedResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count == 1 && cpRecievedPacket.Words.Count >= 2)
            {
                sender.SendQueued(new Packet(false, false, sender.AcquireSequenceNumber, new List<string>() {
                    "login.hashed",
                    GeneratePasswordHash(HashToByteArray(cpRecievedPacket.Words[1]), Password)
                }));
            }
            else if (cpRequestPacket.Words.Count >= 2)
            {
                IsLoggedIn = true;

                if (Login != null)
                {
                    this.Login(this);
                }
            }
        }

        protected virtual void DispatchLogoutResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                IsLoggedIn = false;

                if (Logout != null)
                {
                    this.Logout(this);
                }

                Shutdown();
            }
        }

        protected virtual void DispatchQuitResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                IsLoggedIn = false;

                if (Quit != null)
                {
                    this.Quit(this);
                }

                Shutdown();
            }
        }

        protected virtual void DispatchVersionResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRecievedPacket.Words.Count >= 3)
            {
                if (Version != null)
                {
                    this.Version(this, cpRecievedPacket.Words[1], cpRecievedPacket.Words[2]);
                }
            }
        }

        protected virtual void DispatchEventsEnabledResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (EventsEnabled != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.EventsEnabled(this, Convert.ToBoolean(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.EventsEnabled(this, Convert.ToBoolean(cpRequestPacket.Words[1]));
                    }
                }
            }
        }

        protected virtual void DispatchHelpResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRecievedPacket.Words.Count >= 2)
            {
                List<string> lstWords = cpRecievedPacket.Words;
                lstWords.RemoveAt(0);
                if (Help != null)
                {
                    this.Help(this, lstWords);
                }
            }
        }

        protected virtual void DispatchAdminShutDownResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (ShutdownServer != null)
            {
                if (cpRequestPacket.Words.Count >= 2)
                {
                    this.ShutdownServer(this);
                }
            }
        }

        #endregion

        #region General

        protected virtual void DispatchAdminRunScriptResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 2)
            {
                if (RunScript != null)
                {
                    this.RunScript(this, cpRequestPacket.Words[1]);
                }
            }
        }

        protected virtual void DispatchPunkbusterPbSvCommandResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 2)
            {
                if (SendPunkbusterMessage != null)
                {
                    this.SendPunkbusterMessage(this, cpRequestPacket.Words[1]);
                }
            }
        }

        protected virtual void DispatchServerInfoResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
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
                    "Mappack", // will need to be split into MoHClient and BFBC2Client.
                    "ExternalGameIpandPort",
                    "PunkBusterVersion",
                    "JoinQueueEnabled",
                    "ServerRegion"
                }, cpRecievedPacket.Words.GetRange(1, cpRecievedPacket.Words.Count - 1));

                this.ServerInfo(this, newServerInfo);
            }

            /*
            if (cpRequestPacket.Words.Count >= 1) {

                int iCurrentPlayers = 0, iMaxPlayers = 32, iCurrentRound = 0, iTotalRounds = 0, iTeamScoreScope = 0;
                string sConnectionState = String.Empty;
                List<TeamScore> lstTeamScores;

                // R17
                if (cpRecievedPacket.Words.Count >= 11) {

                    int.TryParse(cpRecievedPacket.Words[2], out iCurrentPlayers);
                    int.TryParse(cpRecievedPacket.Words[3], out iMaxPlayers);
                    int.TryParse(cpRecievedPacket.Words[6], out iCurrentRound);
                    int.TryParse(cpRecievedPacket.Words[7], out iTotalRounds);

                    if (int.TryParse(cpRecievedPacket.Words[8], out iTeamScoreScope) == true) {
                        // include neutral
                        iTeamScoreScope = iTeamScoreScope + 1;
                        lstTeamScores = TeamScore.GetTeamScores(cpRecievedPacket.Words.GetRange(8, iTeamScoreScope));
                    }
                    else {
                        lstTeamScores = TeamScore.GetTeamScores(cpRecievedPacket.Words.GetRange(8, cpRecievedPacket.Words.Count - 8));
                    }
                    if (8 + iTeamScoreScope <= cpRecievedPacket.Words.Count) {
                        sConnectionState = cpRecievedPacket.Words[8 + iTeamScoreScope + 1];
                    }

                    if (this.ServerInfo != null) {
                        this.ServerInfo(this, new CServerInfo(cpRecievedPacket.Words[1], cpRecievedPacket.Words[5], cpRecievedPacket.Words[4], iCurrentPlayers, iMaxPlayers, iCurrentRound, iTotalRounds, lstTeamScores, sConnectionState));
                    }
                }
            }
            */
        }

        protected virtual void DispatchAdminListPlayersResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 2)
            {
                cpRecievedPacket.Words.RemoveAt(0);
                if (ListPlayers != null)
                {
                    List<CPlayerInfo> lstPlayers = CPlayerInfo.GetPlayerList(cpRecievedPacket.Words);
                    var cpsSubset = new CPlayerSubset(cpRequestPacket.Words.GetRange(1, cpRequestPacket.Words.Count - 1));

                    this.ListPlayers(this, lstPlayers, cpsSubset);
                }
            }
        }

        protected virtual void DispatchAdminSayResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 3)
            {
                if (Saying != null)
                {
                    this.Saying(this, cpRequestPacket.Words[1], cpRequestPacket.Words.GetRange(2, cpRequestPacket.Words.Count - 2));
                }
            }
        }

        protected virtual void DispatchAdminYellResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 4)
            {
                if (Yelling != null)
                {
                    this.Yelling(this, cpRequestPacket.Words[1], Convert.ToInt32(cpRequestPacket.Words[2]), cpRequestPacket.Words.GetRange(3, cpRequestPacket.Words.Count - 3));
                }
            }
            // MoHW
            if (cpRequestPacket.Words.Count >= 3 && GameType == "MOHW")
            {
                if (Yelling != null)
                {
                    this.Yelling(this, cpRequestPacket.Words[1], Convert.ToInt32("0"), cpRequestPacket.Words.GetRange(2, cpRequestPacket.Words.Count - 2));
                }
            }
        }

        #endregion

        #region Map Functions

        protected virtual void DispatchAdminRunNextRoundResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (RunNextRound != null)
                {
                    this.RunNextRound(this);
                }
            }
        }

        protected virtual void DispatchAdminCurrentLevelResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1 && cpRecievedPacket.Words.Count >= 2)
            {
                if (CurrentLevel != null)
                {
                    this.CurrentLevel(this, cpRecievedPacket.Words[1]);
                }
            }
        }

        protected virtual void DispatchAdminRestartRoundResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (RestartRound != null)
                {
                    this.RestartRound(this);
                }
            }
        }

        protected virtual void DispatchAdminSupportedMapsResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 2 && cpRecievedPacket.Words.Count > 1)
            {
                if (SupportedMaps != null)
                {
                    this.SupportedMaps(this, cpRequestPacket.Words[1], cpRecievedPacket.Words.GetRange(1, cpRecievedPacket.Words.Count - 1));
                }
            }
        }

        protected virtual void DispatchAdminGetPlaylistsResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                cpRecievedPacket.Words.RemoveAt(0);
                if (ListPlaylists != null)
                {
                    this.ListPlaylists(this, cpRecievedPacket.Words);
                }
            }
        }

        protected virtual void DispatchAdminEndRoundResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 2)
            {
                if (EndRound != null)
                {
                    this.EndRound(this, Convert.ToInt32(cpRequestPacket.Words[1]));
                }
            }
        }

        #endregion

        #region Text Chat Moderation

        protected virtual void DispatchTextChatModerationAddPlayerResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 3)
            {
                if (TextChatModerationListAddPlayer != null)
                {
                    this.TextChatModerationListAddPlayer(this, new TextChatModerationEntry(TextChatModerationEntry.GetPlayerModerationLevelType(cpRequestPacket.Words[1]), cpRequestPacket.Words[2]));
                }
            }
        }

        protected virtual void DispatchTextChatModerationListRemovePlayerResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 2)
            {
                if (TextChatModerationListRemovePlayer != null)
                {
                    this.TextChatModerationListRemovePlayer(this, new TextChatModerationEntry(PlayerModerationLevelType.None, cpRequestPacket.Words[1]));
                }
            }
        }

        protected virtual void DispatchTextChatModerationListClearResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (TextChatModerationListClear != null)
                {
                    this.TextChatModerationListClear(this);
                }
            }
        }

        protected virtual void DispatchTextChatModerationListSaveResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (TextChatModerationListSave != null)
                {
                    this.TextChatModerationListSave(this);
                }
            }
        }

        protected virtual void DispatchTextChatModerationListLoadResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (TextChatModerationListLoad != null)
                {
                    this.TextChatModerationListLoad(this);
                }
            }
        }

        protected virtual void DispatchTextChatModerationListListResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                int requestStartOffset = 0;

                if (cpRequestPacket.Words.Count >= 2)
                {
                    if (int.TryParse(cpRequestPacket.Words[1], out requestStartOffset) == false)
                    {
                        requestStartOffset = 0;
                    }
                }

                if (TextChatModerationListList != null)
                {
                    cpRecievedPacket.Words.RemoveRange(0, 2);

                    var moderationList = new List<TextChatModerationEntry>();

                    for (int i = 0; i + 2 <= cpRecievedPacket.Words.Count;)
                    {
                        moderationList.Add(new TextChatModerationEntry(cpRecievedPacket.Words[i++], cpRecievedPacket.Words[i++]));
                    }

                    this.TextChatModerationListList(this, requestStartOffset, moderationList);
                }
            }
        }

        #endregion

        #region Ban List

        protected virtual void DispatchBanListAddResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 4)
            {
                // banList.add <id-type: id-type> <id: string> <timeout: timeout> <reason: string>
                if (BanListAdd != null)
                {
                    this.BanListAdd(this, new CBanInfo(cpRequestPacket.Words[1], cpRequestPacket.Words[2], new TimeoutSubset(cpRequestPacket.Words.GetRange(3, TimeoutSubset.RequiredLength(cpRequestPacket.Words[3]))), cpRequestPacket.Words.Count >= (4 + TimeoutSubset.RequiredLength(cpRequestPacket.Words[3])) ? cpRequestPacket.Words[(3 + TimeoutSubset.RequiredLength(cpRequestPacket.Words[3]))] : ""));
                }
            }
        }

        protected virtual void DispatchBanListRemoveResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 3)
            {
                if (BanListRemove != null)
                {
                    this.BanListRemove(this, new CBanInfo(cpRequestPacket.Words[1], cpRequestPacket.Words[2]));
                }
            }
        }

        protected virtual void DispatchBanListClearResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (BanListClear != null)
                {
                    this.BanListClear(this);
                }
            }
        }

        protected virtual void DispatchBanListSaveResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (BanListSave != null)
                {
                    this.BanListSave(this);
                }
            }
        }

        protected virtual void DispatchBanListLoadResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (BanListLoad != null)
                {
                    this.BanListLoad(this);
                }
            }
        }

        protected virtual void DispatchBanListListResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
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

                if (BanListList != null)
                {
                    cpRecievedPacket.Words.RemoveAt(0);
                    this.BanListList(this, iRequestStartOffset, CBanInfo.GetVanillaBanlist(cpRecievedPacket.Words, iRequestStartOffset));
                }
            }
        }

        #endregion

        #region Reserved Slots

        protected virtual void DispatchReservedSlotsConfigFileResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (ReservedSlotsConfigFile != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.ReservedSlotsConfigFile(this, cpRecievedPacket.Words[1]);
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.ReservedSlotsConfigFile(this, cpRequestPacket.Words[1]);
                    }
                }
            }
        }

        protected virtual void DispatchReservedSlotsLoadResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (ReservedSlotsLoad != null)
                {
                    this.ReservedSlotsLoad(this);
                }
            }
        }

        protected virtual void DispatchReservedSlotsSaveResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (ReservedSlotsSave != null)
                {
                    this.ReservedSlotsSave(this);
                }
            }
        }

        protected virtual void DispatchReservedSlotsAddPlayerResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 2)
            {
                if (ReservedSlotsPlayerAdded != null)
                {
                    this.ReservedSlotsPlayerAdded(this, cpRequestPacket.Words[1]);
                }
            }
        }

        protected virtual void DispatchReservedSlotsRemovePlayerResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 2)
            {
                if (ReservedSlotsPlayerRemoved != null)
                {
                    this.ReservedSlotsPlayerRemoved(this, cpRequestPacket.Words[1]);
                }
            }
        }

        protected virtual void DispatchReservedSlotsClearResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (ReservedSlotsCleared != null)
                {
                    this.ReservedSlotsCleared(this);
                }
            }
        }

        protected virtual void DispatchReservedSlotsListResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count == 1)
            {
                cpRecievedPacket.Words.RemoveAt(0);
                if (ReservedSlotsList != null)
                {
                    this.ReservedSlotsList(this, cpRecievedPacket.Words);
                }
            }
        }

        #endregion

        #region Spectator List

        protected virtual void DispatchSpectatorListConfigFileResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (SpectatorListConfigFile != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.SpectatorListConfigFile(this, cpRecievedPacket.Words[1]);
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.SpectatorListConfigFile(this, cpRequestPacket.Words[1]);
                    }
                }
            }
        }

        protected virtual void DispatchSpectatorListLoadResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (SpectatorListLoad != null)
                {
                    this.SpectatorListLoad(this);
                }
            }
        }

        protected virtual void DispatchSpectatorListSaveResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (SpectatorListSave != null)
                {
                    this.SpectatorListSave(this);
                }
            }
        }

        protected virtual void DispatchSpectatorListAddPlayerResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 2)
            {
                if (SpectatorListPlayerAdded != null)
                {
                    this.SpectatorListPlayerAdded(this, cpRequestPacket.Words[1]);
                }
            }
        }

        protected virtual void DispatchSpectatorListRemovePlayerResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 2)
            {
                if (SpectatorListPlayerRemoved != null)
                {
                    this.SpectatorListPlayerRemoved(this, cpRequestPacket.Words[1]);
                }
            }
        }

        protected virtual void DispatchSpectatorListClearResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (SpectatorListCleared != null)
                {
                    this.SpectatorListCleared(this);
                }
            }
        }

        protected virtual void DispatchSpectatorListListResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count == 1)
            {
                cpRecievedPacket.Words.RemoveAt(0);
                if (SpectatorListList != null)
                {
                    this.SpectatorListList(this, cpRecievedPacket.Words);
                }
            }
        }

        #endregion



        #region Game Admin List

        protected virtual void DispatchGameAdminConfigFileResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (GameAdminConfigFile != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.GameAdminConfigFile(this, cpRecievedPacket.Words[1]);
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.GameAdminConfigFile(this, cpRequestPacket.Words[1]);
                    }
                }
            }
        }

        protected virtual void DispatchGameAdminLoadResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (GameAdminLoad != null)
                {
                    this.GameAdminLoad(this);
                }
            }
        }

        protected virtual void DispatchGameAdminSaveResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (GameAdminSave != null)
                {
                    this.GameAdminSave(this);
                }
            }
        }

        protected virtual void DispatchGameAdminAddPlayerResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 2)
            {
                if (GameAdminPlayerAdded != null)
                {
                    this.GameAdminPlayerAdded(this, cpRequestPacket.Words[1]);
                }
            }
        }

        protected virtual void DispatchGameAdminRemovePlayerResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 2)
            {
                if (GameAdminPlayerRemoved != null)
                {
                    this.GameAdminPlayerRemoved(this, cpRequestPacket.Words[1]);
                }
            }
        }

        protected virtual void DispatchGameAdminClearResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (GameAdminCleared != null)
                {
                    this.GameAdminCleared(this);
                }
            }
        }

        protected virtual void DispatchGameAdminListResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count == 1)
            {
                cpRecievedPacket.Words.RemoveAt(0);
                if (GameAdminList != null)
                {
                    this.GameAdminList(this, cpRecievedPacket.Words);
                }
            }
        }

        #endregion


        #region Map List

        protected virtual void DispatchMapListConfigFileResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (MapListConfigFile != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.MapListConfigFile(this, cpRecievedPacket.Words[1]);
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.MapListConfigFile(this, cpRequestPacket.Words[1]);
                    }
                }
            }
        }

        protected virtual void DispatchMapListLoadResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (MapListLoad != null)
                {
                    this.MapListLoad(this);
                    SendMapListListRoundsPacket();
                }
            }
        }

        protected virtual void DispatchMapListSaveResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (MapListSave != null)
                {
                    this.MapListSave(this);
                }
            }
        }

        protected virtual void DispatchMapListListResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                cpRecievedPacket.Words.RemoveAt(0);

                var lstMaplist = new List<MaplistEntry>();

                if (cpRequestPacket.Words.Count == 1)
                {
                    lstMaplist.AddRange(cpRecievedPacket.Words.Select(t => new MaplistEntry(t)));
                }
                else if (cpRequestPacket.Words.Count >= 2 && String.Compare(cpRequestPacket.Words[1], "rounds", System.StringComparison.OrdinalIgnoreCase) == 0)
                {
                    for (int i = 0; i + 1 < cpRecievedPacket.Words.Count; i = i + 2)
                    {
                        int rounds = 0;

                        if (int.TryParse(cpRecievedPacket.Words[i + 1], out rounds) == true)
                        {
                            lstMaplist.Add(new MaplistEntry(cpRecievedPacket.Words[i], rounds));
                        }
                    }
                }

                if (MapListListed != null)
                {
                    this.MapListListed(this, lstMaplist);
                }
            }
        }

        protected virtual void DispatchMapListClearResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (MapListCleared != null)
                {
                    this.MapListCleared(this);
                }
            }
        }

        protected virtual void DispatchMapListAppendResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 2)
            {
                MaplistEntry mapEntry = null;

                int iRounds = 0;

                if (cpRequestPacket.Words.Count == 2)
                {
                    mapEntry = new MaplistEntry(cpRequestPacket.Words[1]);
                }
                else if (cpRequestPacket.Words.Count >= 3 && int.TryParse(cpRequestPacket.Words[2], out iRounds) == true)
                {
                    mapEntry = new MaplistEntry(cpRequestPacket.Words[1], iRounds);
                }

                if (MapListMapAppended != null)
                {
                    this.MapListMapAppended(this, mapEntry);
                }
            }
        }

        protected virtual void DispatchMapListNextLevelIndexResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (MapListNextLevelIndex != null)
                {
                    int mapIndex = 0;

                    if ((cpRequestPacket.Words.Count >= 2 && int.TryParse(cpRequestPacket.Words[1], out mapIndex) == true) || cpRecievedPacket.Words.Count >= 2 && int.TryParse(cpRecievedPacket.Words[1], out mapIndex) == true)
                    {
                        if (this.MapListNextLevelIndex != null)
                        {
                            this.MapListNextLevelIndex(this, mapIndex);
                        }
                    }
                }
            }
        }

        protected virtual void DispatchMapListGetMapIndicesResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                int iMapIndex = 0;
                int iNextIndex = 0;
                //if (this.MapListGetMapIndices != null)
                //{
                if (cpRecievedPacket.Words.Count >= 2 && (int.TryParse(cpRecievedPacket.Words[1], out iMapIndex) == true && int.TryParse(cpRecievedPacket.Words[2], out iNextIndex) == true))
                {
                    if (this.MapListGetMapIndices != null)
                    {
                        this.MapListGetMapIndices(this, iMapIndex, iNextIndex);
                    }
                }
                //}
            }
        }

        protected virtual void DispatchMapListGetRoundsResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                int iCurrentRound = 0;
                int iTotalRounds = 0;
                if (cpRecievedPacket.Words.Count >= 2 && (int.TryParse(cpRecievedPacket.Words[1], out iCurrentRound) == true && int.TryParse(cpRecievedPacket.Words[2], out iTotalRounds) == true))
                {
                    this.MapListGetRounds(this, iCurrentRound, iTotalRounds);
                }
            }
        }

        protected virtual void DispatchMapListRemoveResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 2)
            {
                int iMapIndex = 0;
                if (int.TryParse(cpRequestPacket.Words[1], out iMapIndex) == true)
                {
                    if (MapListMapRemoved != null)
                    {
                        this.MapListMapRemoved(this, iMapIndex);
                    }
                }
            }
        }

        protected virtual void DispatchMapListInsertResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
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

        #region Vars

        protected virtual void DispatchVarsAdminPasswordResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (AdminPassword != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.AdminPassword(this, cpRecievedPacket.Words[1]);
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.AdminPassword(this, cpRequestPacket.Words[1]);
                    }
                }
            }
        }

        protected virtual void DispatchVarsGamePasswordResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (GamePassword != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.GamePassword(this, cpRecievedPacket.Words[1]);
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.GamePassword(this, cpRequestPacket.Words[1]);
                    }
                }
            }
        }

        protected virtual void DispatchVarsPunkbusterResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (Punkbuster != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.Punkbuster(this, Convert.ToBoolean(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.Punkbuster(this, Convert.ToBoolean(cpRequestPacket.Words[1]));
                    }
                }
            }
        }

        protected virtual void DispatchVarsFairFightStatusResponse(FrostbiteConnection sender, Packet recievedPacket, Packet requestPacket)
        {
            if (requestPacket.Words.Count >= 1)
            {
                if (FairFight != null)
                {
                    if (recievedPacket.Words.Count == 2)
                    {
                        this.FairFight(this, Convert.ToBoolean(recievedPacket.Words[1]));
                    }
                    else if (requestPacket.Words.Count >= 2)
                    {
                        this.FairFight(this, Convert.ToBoolean(requestPacket.Words[1]));
                    }
                }
            }
        }

        protected virtual void DispatchVarsMaxSpectatorsResponse(FrostbiteConnection sender, Packet recievedPacket, Packet requestPacket)
        {
            if (requestPacket.Words.Count >= 1)
            {
                if (MaxSpectators != null)
                {
                    if (recievedPacket.Words.Count == 2)
                    {
                        this.MaxSpectators(this, Convert.ToInt32(recievedPacket.Words[1]));
                    }
                    else if (requestPacket.Words.Count >= 2)
                    {
                        this.MaxSpectators(this, Convert.ToInt32(requestPacket.Words[1]));
                    }
                }
            }
        }

        protected virtual void DispatchVarsHardCoreResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (Hardcore != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.Hardcore(this, Convert.ToBoolean(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.Hardcore(this, Convert.ToBoolean(cpRequestPacket.Words[1]));
                    }
                }
            }
        }

        protected virtual void DispatchVarsRankedResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (Ranked != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.Ranked(this, Convert.ToBoolean(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.Ranked(this, Convert.ToBoolean(cpRequestPacket.Words[1]));
                    }
                }
            }
        }

        protected virtual void DispatchVarsFriendlyFireResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (FriendlyFire != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.FriendlyFire(this, Convert.ToBoolean(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.FriendlyFire(this, Convert.ToBoolean(cpRequestPacket.Words[1]));
                    }
                }
            }
        }

        protected virtual void DispatchVarsPlayerLimitResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (PlayerLimit != null)
                {
                    if (cpRecievedPacket.Words.Count == 2 && cpRecievedPacket.Words[1].Length <= 3)
                    {
                        this.PlayerLimit(this, Convert.ToInt32(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2 && cpRequestPacket.Words[1].Length <= 3)
                    {
                        this.PlayerLimit(this, Convert.ToInt32(cpRequestPacket.Words[1]));
                    }
                }
            }
        }

        protected virtual void DispatchVarsCurrentPlayerLimitResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (cpRecievedPacket.Words.Count == 2 && CurrentPlayerLimit != null)
                {
                    this.CurrentPlayerLimit(this, Convert.ToInt32(cpRecievedPacket.Words[1]));
                }
            }
        }

        protected virtual void DispatchVarsMaxPlayerLimitResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (cpRecievedPacket.Words.Count == 2 && MaxPlayerLimit != null)
                {
                    this.MaxPlayerLimit(this, Convert.ToInt32(cpRecievedPacket.Words[1]));
                }
            }
        }

        protected virtual void DispatchVarsBannerUrlResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (BannerUrl != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.BannerUrl(this, cpRecievedPacket.Words[1]);
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.BannerUrl(this, cpRequestPacket.Words[1]);
                    }
                }
            }
        }

        protected virtual void DispatchVarsServerDescriptionResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (ServerDescription != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.ServerDescription(this, cpRecievedPacket.Words[1]);
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.ServerDescription(this, cpRequestPacket.Words[1]);
                    }
                }
            }
        }

        protected virtual void DispatchVarsTeamKillCountForKickResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (TeamKillCountForKick != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.TeamKillCountForKick(this, (int)Convert.ToDecimal(cpRecievedPacket.Words[1], CultureInfo.InvariantCulture));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.TeamKillCountForKick(this, (int)Convert.ToDecimal(cpRequestPacket.Words[1], CultureInfo.InvariantCulture));
                    }
                }
            }
        }

        protected virtual void DispatchVarsTeamKillKickForBanResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (TeamKillKickForBan != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.TeamKillKickForBan(this, (int)Convert.ToDecimal(cpRecievedPacket.Words[1], CultureInfo.InvariantCulture));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.TeamKillKickForBan(this, (int)Convert.ToDecimal(cpRequestPacket.Words[1], CultureInfo.InvariantCulture));
                    }
                }
            }
        }

        protected virtual void DispatchVarsTeamKillValueForKickResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (TeamKillValueForKick != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.TeamKillValueForKick(this, (int)Convert.ToDecimal(cpRecievedPacket.Words[1], CultureInfo.InvariantCulture));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.TeamKillValueForKick(this, (int)Convert.ToDecimal(cpRequestPacket.Words[1], CultureInfo.InvariantCulture));
                    }
                }
            }
        }

        protected virtual void DispatchVarsTeamKillValueIncreaseResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (TeamKillValueIncrease != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.TeamKillValueIncrease(this, (int)Convert.ToDecimal(cpRecievedPacket.Words[1], CultureInfo.InvariantCulture));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.TeamKillValueIncrease(this, (int)Convert.ToDecimal(cpRequestPacket.Words[1], CultureInfo.InvariantCulture));
                    }
                }
            }
        }

        protected virtual void DispatchVarsTeamKillValueDecreasePerSecondResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (TeamKillValueDecreasePerSecond != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.TeamKillValueDecreasePerSecond(this, (int)Convert.ToDecimal(cpRecievedPacket.Words[1], CultureInfo.InvariantCulture));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.TeamKillValueDecreasePerSecond(this, (int)Convert.ToDecimal(cpRequestPacket.Words[1], CultureInfo.InvariantCulture));
                    }
                }
            }
        }

        protected virtual void DispatchVarsIdleTimeoutResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (IdleTimeout != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.IdleTimeout(this, Convert.ToInt32(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.IdleTimeout(this, Convert.ToInt32(cpRequestPacket.Words[1]));
                    }
                }
            }
        }

        protected virtual void DispatchVarsProfanityFilterResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (ProfanityFilter != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.ProfanityFilter(this, Convert.ToBoolean(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.ProfanityFilter(this, Convert.ToBoolean(cpRequestPacket.Words[1]));
                    }
                }
            }
        }

        protected virtual void DispatchVarsServerNameResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (ServerName != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.ServerName(this, cpRecievedPacket.Words[1]);
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.ServerName(this, cpRequestPacket.Words[1]);
                    }
                }
            }
        }

        protected virtual void DispatchVarsTextChatSpamTriggerCountResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (TextChatSpamTriggerCount != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.TextChatSpamTriggerCount(this, Convert.ToInt32(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.TextChatSpamTriggerCount(this, Convert.ToInt32(cpRequestPacket.Words[1]));
                    }
                }
            }
        }

        protected virtual void DispatchVarsTextChatSpamDetectionTimeResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (TextChatSpamDetectionTime != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.TextChatSpamDetectionTime(this, Convert.ToInt32(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.TextChatSpamDetectionTime(this, Convert.ToInt32(cpRequestPacket.Words[1]));
                    }
                }
            }
        }

        protected virtual void DispatchVarsTextChatSpamCoolDownTimeResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (TextChatSpamCoolDownTime != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.TextChatSpamCoolDownTime(this, Convert.ToInt32(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.TextChatSpamCoolDownTime(this, Convert.ToInt32(cpRequestPacket.Words[1]));
                    }
                }
            }
        }

        protected virtual void DispatchVarsTextChatModerationModeResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (TextChatModerationMode != null)
                {
                    if (cpRecievedPacket.Words.Count == 2)
                    {
                        this.TextChatModerationMode(this, TextChatModerationEntry.GetServerModerationLevelType(cpRecievedPacket.Words[1]));
                    }
                    else if (cpRequestPacket.Words.Count >= 2)
                    {
                        this.TextChatModerationMode(this, TextChatModerationEntry.GetServerModerationLevelType(cpRequestPacket.Words[1]));
                    }
                }
            }
        }

        #endregion

        #region Level Variables

        protected virtual void DispatchLevelVarsSetResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 2)
            {
                if (LevelVariablesSet != null)
                {
                    this.LevelVariablesSet(this, LevelVariable.ExtractContextVariable(false, cpRequestPacket.Words.GetRange(1, cpRequestPacket.Words.Count - 1)));
                }
            }
        }

        protected virtual void DispatchLevelVarsGetResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRecievedPacket.Words.Count >= 2 && cpRequestPacket.Words.Count >= 2)
            {
                LevelVariable request = LevelVariable.ExtractContextVariable(false, cpRequestPacket.Words.GetRange(1, cpRequestPacket.Words.Count - 1));

                request.RawValue = cpRecievedPacket.Words[1];

                if (LevelVariablesGet != null)
                {
                    this.LevelVariablesGet(this, request, null);
                }
            }
        }

        protected virtual void DispatchLevelVarsEvaluateResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRecievedPacket.Words.Count >= 2 && cpRequestPacket.Words.Count >= 2)
            {
                var request = new LevelVariable(new LevelVariableContext(String.Empty, String.Empty), cpRequestPacket.Words[1], cpRecievedPacket.Words[1]);

                if (LevelVariablesEvaluate != null)
                {
                    this.LevelVariablesEvaluate(this, request, null);
                }
            }
        }

        protected virtual void DispatchLevelVarsClearResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 2)
            {
                LevelVariable request = LevelVariable.ExtractContextVariable(false, cpRequestPacket.Words.GetRange(1, cpRequestPacket.Words.Count - 1));

                if (LevelVariablesClear != null)
                {
                    this.LevelVariablesClear(this, request);
                }
            }
        }

        protected virtual void DispatchLevelVarsListResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRecievedPacket.Words.Count >= 2 && cpRequestPacket.Words.Count >= 2)
            {
                LevelVariable varRequestContext = LevelVariable.ExtractContextVariable(false, cpRequestPacket.Words.GetRange(1, cpRequestPacket.Words.Count - 1));

                var lstVariables = new List<LevelVariable>();

                int iMatchedVariables = 0;

                if (int.TryParse(cpRecievedPacket.Words[1], out iMatchedVariables) == true)
                {
                    for (int i = 0; i < iMatchedVariables && ((i + 1) * 4 + 2) <= cpRecievedPacket.Words.Count; i++)
                    {
                        lstVariables.Add(LevelVariable.ExtractContextVariable(true, cpRecievedPacket.Words.GetRange(i * 4 + 2, 4)));
                    }

                    if (LevelVariablesList != null)
                    {
                        this.LevelVariablesList(this, varRequestContext, lstVariables);
                    }
                }
            }
        }

        #endregion

        #region Player Events

        protected virtual void DispatchAdminKickPlayerResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (PlayerKickedByAdmin != null)
            {
                if (cpRequestPacket.Words.Count >= 2)
                {
                    this.PlayerKickedByAdmin(this, cpRequestPacket.Words[1], cpRequestPacket.Words[2]);
                }
                else if (cpRequestPacket.Words.Count >= 2)
                {
                    this.PlayerKickedByAdmin(this, cpRequestPacket.Words[1], String.Empty);
                }
            }
        }

        protected virtual void DispatchAdminMovePlayerResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (PlayerMovedByAdmin != null)
            {
                if (cpRequestPacket.Words.Count >= 5)
                {
                    int desinationTeamId;
                    int destinationSquadId;
                    bool forceKilled;

                    if (int.TryParse(cpRequestPacket.Words[2], out desinationTeamId) == true && int.TryParse(cpRequestPacket.Words[3], out destinationSquadId) == true && bool.TryParse(cpRequestPacket.Words[4], out forceKilled) == true)
                    {
                        this.PlayerMovedByAdmin(this, cpRequestPacket.Words[1], desinationTeamId, destinationSquadId, forceKilled);
                    }
                }
            }
        }

        protected virtual void DispatchAdminKillPlayerResponse(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (PlayerKilledByAdmin != null)
            {
                if (cpRequestPacket.Words.Count >= 2)
                {
                    this.PlayerKilledByAdmin(this, cpRequestPacket.Words[1]);
                }
            }
        }

        #endregion

        // Command
        public virtual void DispatchResponsePacket(FrostbiteConnection sender, Packet cpRecievedPacket, Packet cpRequestPacket)
        {
            if (cpRecievedPacket.Words.Count >= 1 && String.Compare(cpRecievedPacket.Words[0], "OK", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (ResponseDelegates.ContainsKey(cpRequestPacket.Words[0]) == true)
                {
                    ResponseDelegates[cpRequestPacket.Words[0]](sender, cpRecievedPacket, cpRequestPacket);
                }
            }
            else if (cpRecievedPacket.Words.Count >= 1 && (String.Compare(cpRecievedPacket.Words[0], "InvalidPassword", StringComparison.OrdinalIgnoreCase) == 0 || String.Compare(cpRecievedPacket.Words[0], "InvalidPasswordHash", StringComparison.OrdinalIgnoreCase) == 0) && cpRequestPacket.Words.Count >= 1 && (String.Compare(cpRequestPacket.Words[0], "login.hashed", StringComparison.OrdinalIgnoreCase) == 0 || String.Compare(cpRequestPacket.Words[0], "login.plaintext", StringComparison.OrdinalIgnoreCase) == 0))
            {
                IsLoggedIn = false;

                if (LoginFailure != null)
                {
                    this.LoginFailure(this, cpRecievedPacket.Words[0]);
                }

                Shutdown();
            }
            else if (cpRecievedPacket.Words.Count >= 3 && String.Compare(cpRecievedPacket.Words[0], "ScriptError", StringComparison.OrdinalIgnoreCase) == 0 && cpRequestPacket.Words.Count >= 2)
            {
                if (RunScriptError != null)
                {
                    this.RunScriptError(this, "", Convert.ToInt32(cpRecievedPacket.Words[1]), cpRecievedPacket.Words[2]);
                }
            }
            else if (cpRecievedPacket.Words.Count == 1 && String.Compare(cpRecievedPacket.Words[0], "ExecutedOnNextRound", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (ResponseDelegates.ContainsKey(cpRequestPacket.Words[0]) == true)
                {
                    ResponseDelegates[cpRequestPacket.Words[0]](sender, cpRecievedPacket, cpRequestPacket);
                }
            }
            // Else it is an error..
            else
            {
                // InvalidArguments
                // TooLongMessage
                // InvalidDuration
                // InvalidFileName
                // InvalidLevelName
                // More...
                if (ResponseError != null)
                {
                    this.ResponseError(this, cpRequestPacket, cpRecievedPacket.Words[0]);
                }
            }
        }

        #endregion

        #region Request Handlers

        #region Player Initiated Events

        protected virtual void DispatchPlayerOnJoinRequest(FrostbiteConnection sender, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 2)
            {
                if (PlayerJoin != null && cpRequestPacket.Words[1].Length > 0)
                {
                    this.PlayerJoin(this, cpRequestPacket.Words[1]);
                }
            }
        }

        protected virtual void DispatchPlayerOnLeaveRequest(FrostbiteConnection sender, Packet cpRequestPacket)
        {
            // Backwards compatability
            //else if (cpRequestPacket.Words.Count == 2 && String.Compare(cpRequestPacket.Words[0], "player.onLeave", true) == 0) {
            //    if (this.PlayerLeft != null) {
            //        this.PlayerLeft(this, cpRequestPacket.Words[1], null);
            //    }
            //}
            if (cpRequestPacket.Words.Count >= 3)
            {
                if (PlayerLeft != null)
                {
                    CPlayerInfo cpiPlayer = null;

                    List<CPlayerInfo> lstPlayers = CPlayerInfo.GetPlayerList(cpRequestPacket.Words.GetRange(2, cpRequestPacket.Words.Count - 2));

                    if (lstPlayers.Count > 0)
                    {
                        cpiPlayer = lstPlayers[0];
                    }

                    this.PlayerLeft(this, cpRequestPacket.Words[1], cpiPlayer);
                }
            }
        }

        protected virtual void DispatchPlayerOnDisconnectRequest(FrostbiteConnection sender, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 3)
            {
                if (PlayerDisconnected != null)
                {
                    this.PlayerDisconnected(this, cpRequestPacket.Words[1], cpRequestPacket.Words[2]);
                }
            }
        }

        protected virtual void DispatchPlayerOnAuthenticatedRequest(FrostbiteConnection sender, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 3)
            {
                if (PlayerAuthenticated != null)
                {
                    this.PlayerAuthenticated(this, cpRequestPacket.Words[1], cpRequestPacket.Words[2]);
                }
            }
        }

        protected virtual void DispatchPlayerOnKillRequest(FrostbiteConnection sender, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 11)
            {
                if (PlayerKilled != null)
                {
                    bool headshot = false;

                    if (bool.TryParse(cpRequestPacket.Words[4], out headshot) == true)
                    {
                        this.PlayerKilled(this, cpRequestPacket.Words[1], cpRequestPacket.Words[2], cpRequestPacket.Words[3], headshot, new Point3D(cpRequestPacket.Words[5], cpRequestPacket.Words[7], cpRequestPacket.Words[6]), new Point3D(cpRequestPacket.Words[8], cpRequestPacket.Words[10], cpRequestPacket.Words[9]));
                    }
                }
            }
        }

        protected virtual void DispatchPlayerOnChatRequest(FrostbiteConnection sender, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 3)
            {
                int iTeamID = 0, iSquadID = 0;

                if (cpRequestPacket.Words.Count == 3)
                {
                    // < R9 Support.
                    if (GlobalChat != null)
                    {
                        this.GlobalChat(this, cpRequestPacket.Words[1], cpRequestPacket.Words[2]);
                    }
                    if (TeamChat != null)
                    {
                        this.TeamChat(this, cpRequestPacket.Words[1], cpRequestPacket.Words[2], 0);
                    }
                    if (SquadChat != null)
                    {
                        this.SquadChat(this, cpRequestPacket.Words[1], cpRequestPacket.Words[2], 0, 0);
                    }
                }
                else if (cpRequestPacket.Words.Count == 4 && (String.Compare(cpRequestPacket.Words[3], "all", StringComparison.OrdinalIgnoreCase) == 0 || String.Compare(cpRequestPacket.Words[3], "unknown", StringComparison.OrdinalIgnoreCase) == 0))
                {
                    // "unknown" because of BF4 beta
                    if (GlobalChat != null)
                    {
                        this.GlobalChat(this, cpRequestPacket.Words[1], cpRequestPacket.Words[2]);
                    }
                }
                else if (cpRequestPacket.Words.Count >= 5 && String.Compare(cpRequestPacket.Words[3], "team", StringComparison.OrdinalIgnoreCase) == 0 && int.TryParse(cpRequestPacket.Words[4], out iTeamID) == true)
                {
                    if (this.TeamChat != null)
                    {
                        this.TeamChat(this, cpRequestPacket.Words[1], cpRequestPacket.Words[2], iTeamID);
                    }
                }
                else if (cpRequestPacket.Words.Count >= 5 && String.Compare(cpRequestPacket.Words[3], "player", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    if (this.PlayerChat != null)
                    {
                        this.PlayerChat(this, cpRequestPacket.Words[1], cpRequestPacket.Words[2], cpRequestPacket.Words[4]);
                    }
                }
                else if (cpRequestPacket.Words.Count >= 6 && String.Compare(cpRequestPacket.Words[3], "squad", StringComparison.OrdinalIgnoreCase) == 0 && int.TryParse(cpRequestPacket.Words[4], out iTeamID) == true && int.TryParse(cpRequestPacket.Words[5], out iSquadID) == true)
                {
                    if (SquadChat != null)
                    {
                        this.SquadChat(this, cpRequestPacket.Words[1], cpRequestPacket.Words[2], iTeamID, iSquadID);
                    }
                }

                cpRequestPacket.Words.RemoveAt(0);
                if (Chat != null)
                {
                    this.Chat(this, cpRequestPacket.Words);
                }
            }
        }

        protected virtual void DispatchPlayerOnKickedRequest(FrostbiteConnection sender, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 3)
            {
                if (PlayerKicked != null)
                {
                    this.PlayerKicked(this, cpRequestPacket.Words[1], cpRequestPacket.Words[2]);
                }
            }
        }

        protected virtual void DispatchPlayerOnTeamChangeRequest(FrostbiteConnection sender, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 4)
            {
                int iTeamID = 0, iSquadID = 0;

                // TO DO: Specs say TeamId and SquadId which is a little odd..
                if (int.TryParse(cpRequestPacket.Words[2], out iTeamID) == true && int.TryParse(cpRequestPacket.Words[3], out iSquadID) == true)
                {
                    if (PlayerChangedTeam != null)
                    {
                        this.PlayerChangedTeam(this, cpRequestPacket.Words[1], iTeamID, iSquadID);
                    }
                }
            }
        }

        protected virtual void DispatchPlayerOnSquadChangeRequest(FrostbiteConnection sender, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 4)
            {
                int iTeamID = 0, iSquadID = 0;

                if (int.TryParse(cpRequestPacket.Words[2], out iTeamID) == true && int.TryParse(cpRequestPacket.Words[3], out iSquadID) == true)
                {
                    if (PlayerChangedSquad != null)
                    {
                        this.PlayerChangedSquad(this, cpRequestPacket.Words[1], iTeamID, iSquadID);
                    }
                }
            }
        }

        protected virtual void DispatchPlayerOnSpawnRequest(FrostbiteConnection sender, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 9)
            {
                if (PlayerSpawned != null)
                {
                    this.PlayerSpawned(this, cpRequestPacket.Words[1], cpRequestPacket.Words[2], cpRequestPacket.Words.GetRange(3, 3), cpRequestPacket.Words.GetRange(6, 3)); // new Inventory(cpRequestPacket.Words[3], cpRequestPacket.Words[4], cpRequestPacket.Words[5], cpRequestPacket.Words[6], cpRequestPacket.Words[7], cpRequestPacket.Words[8]));
                }
            }
        }

        #endregion

        #region Server Initiated Events

        protected virtual void DispatchServerOnLoadingLevelRequest(FrostbiteConnection sender, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 4)
            {
                if (LoadingLevel != null)
                {
                    int iRoundsPlayed = 0, iRoundsTotal = 0;

                    if (int.TryParse(cpRequestPacket.Words[2], out iRoundsPlayed) == true && int.TryParse(cpRequestPacket.Words[3], out iRoundsTotal) == true)
                    {
                        this.LoadingLevel(this, cpRequestPacket.Words[1], iRoundsPlayed, iRoundsTotal);
                    }
                }
            }
        }

        protected virtual void DispatchServerOnLevelLoadedRequest(FrostbiteConnection sender, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 5)
            {
                if (LevelLoaded != null)
                {
                    int iRoundsPlayed = 0, iRoundsTotal = 0;

                    if (int.TryParse(cpRequestPacket.Words[3], out iRoundsPlayed) == true && int.TryParse(cpRequestPacket.Words[4], out iRoundsTotal) == true)
                    {
                        this.LevelLoaded(this, cpRequestPacket.Words[1], cpRequestPacket.Words[2], iRoundsPlayed, iRoundsTotal);
                    }
                }
            }
        }

        protected virtual void DispatchServerOnLevelStartedRequest(FrostbiteConnection sender, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (LevelStarted != null)
                {
                    this.LevelStarted(this);
                }
            }
        }

        protected virtual void DispatchServerOnRoundOverRequest(FrostbiteConnection sender, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 2)
            {
                int iTeamID = 0;

                if (int.TryParse(cpRequestPacket.Words[1], out iTeamID) == true)
                {
                    if (RoundOver != null)
                    {
                        this.RoundOver(this, iTeamID);
                    }
                }
            }
        }

        protected virtual void DispatchServerOnRoundOverPlayersRequest(FrostbiteConnection sender, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 2)
            {
                cpRequestPacket.Words.RemoveAt(0);
                if (RoundOverPlayers != null)
                {
                    this.RoundOverPlayers(this, CPlayerInfo.GetPlayerList(cpRequestPacket.Words));
                }
            }
        }

        protected virtual void DispatchServerOnRoundOverTeamScoresRequest(FrostbiteConnection sender, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 2)
            {
                cpRequestPacket.Words.RemoveAt(0);
                if (RoundOverTeamScores != null)
                {
                    this.RoundOverTeamScores(this, TeamScore.GetTeamScores(cpRequestPacket.Words));
                }
            }
        }

        #endregion

        #region Punkbuster Initiated Events

        protected virtual void DispatchPunkBusterOnMessageRequest(FrostbiteConnection sender, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 2)
            {
                if (PunkbusterMessage != null)
                {
                    this.PunkbusterMessage(this, cpRequestPacket.Words[1]);
                }
            }
        }

        #endregion

        // Events
        public virtual void DispatchRequestPacket(FrostbiteConnection sender, Packet cpRequestPacket)
        {
            if (cpRequestPacket.Words.Count >= 1)
            {
                if (RequestDelegates.ContainsKey(cpRequestPacket.Words[0]) == true)
                {
                    RequestDelegates[cpRequestPacket.Words[0]](sender, cpRequestPacket);
                }
            }
        }

        #endregion

        #region Events

        #region Server Request

        #region Player

        public virtual event PlayerEventHandler PlayerJoin;
        public virtual event PlayerLeaveHandler PlayerLeft;
        public virtual event PlayerDisconnectedHandler PlayerDisconnected;
        public virtual event PlayerAuthenticatedHandler PlayerAuthenticated;
        public virtual event PlayerKickedHandler PlayerKicked;
        public virtual event PlayerTeamChangeHandler PlayerChangedTeam;
        public virtual event PlayerTeamChangeHandler PlayerChangedSquad;
        public virtual event PlayerKilledHandler PlayerKilled;
        public virtual event PlayerSpawnedHandler PlayerSpawned;

        #endregion

        #region Synthetic

        public virtual event PlayerKickedHandler PlayerKickedByAdmin;
        public virtual event PlayerKilledByAdminHandler PlayerKilledByAdmin;
        public virtual event PlayerMovedByAdminHandler PlayerMovedByAdmin;

        #endregion

        #region Chat

        public virtual event RawChatHandler Chat;
        public virtual event GlobalChatHandler GlobalChat;
        public virtual event TeamChatHandler TeamChat;
        public virtual event SquadChatHandler SquadChat;
        public virtual event PlayerChatHandler PlayerChat;

        #endregion

        #region Punkbuster

        public virtual event PunkbusterMessageHandler PunkbusterMessage;

        #endregion

        #region Map/Round

        public virtual event LoadingLevelHandler LoadingLevel;
        public virtual event EmptyParamterHandler LevelStarted;
        public virtual event LevelLoadedHandler LevelLoaded;
        public virtual event RoundOverHandler RoundOver;
        public virtual event RoundOverPlayersHandler RoundOverPlayers;
        public virtual event RoundOverTeamScoresHandler RoundOverTeamScores;

        #endregion

        #endregion

        #region Client Responses

        #region Global

        public virtual event ResponseErrorHandler ResponseError;
        public virtual event RunScriptHandler RunScript;
        public virtual event RunScriptErrorHandler RunScriptError;
        public virtual event EmptyParamterHandler ShutdownServer;

        #endregion

        #region Punkbuster

        public virtual event SendPunkBusterMessageHandler SendPunkbusterMessage;

        #endregion

        #region Query

        public virtual event ServerInfoHandler ServerInfo;
        public virtual event ListPlayersHandler ListPlayers;

        #endregion

        #region Communication

        public virtual event YellingHandler Yelling;
        public virtual event SayingHandler Saying;

        #endregion

        #region Map/Round

        public virtual event EmptyParamterHandler RunNextRound; // Alias for runNextRound
        public virtual event CurrentLevelHandler CurrentLevel;
        public virtual event EmptyParamterHandler RestartRound; // Alias for restartRound
        public virtual event SupportedMapsHandler SupportedMaps;
        public virtual event EndRoundHandler EndRound;

        // Playlist
        public virtual event ListPlaylistsHandler ListPlaylists;

        #region BFBC2 Specific

        public virtual event PlaylistSetHandler PlaylistSet;

        #endregion

        #endregion

        #region Ban list

        public virtual event EmptyParamterHandler BanListLoad;
        public virtual event EmptyParamterHandler BanListSave;
        public virtual event BanListAddHandler BanListAdd;
        public virtual event BanListRemoveHandler BanListRemove;
        public virtual event EmptyParamterHandler BanListClear;
        public virtual event BanListListHandler BanListList;

        #endregion

        #region Text Chat Moderation

        public virtual event EmptyParamterHandler TextChatModerationListLoad;
        public virtual event EmptyParamterHandler TextChatModerationListSave;
        public virtual event TextChatModerationListAddPlayerHandler TextChatModerationListAddPlayer;
        public virtual event TextChatModerationListRemovePlayerHandler TextChatModerationListRemovePlayer;
        public virtual event EmptyParamterHandler TextChatModerationListClear;
        public virtual event TextChatModerationListListHandler TextChatModerationListList;

        #endregion

        #region Reserved Slots

        public virtual event ReserverdSlotsConfigFileHandler ReservedSlotsConfigFile;
        public virtual event EmptyParamterHandler ReservedSlotsLoad;
        public virtual event EmptyParamterHandler ReservedSlotsSave;
        public virtual event ReservedSlotsPlayerHandler ReservedSlotsPlayerAdded;
        public virtual event ReservedSlotsPlayerHandler ReservedSlotsPlayerRemoved;
        public virtual event EmptyParamterHandler ReservedSlotsCleared;
        public virtual event ReservedSlotsListHandler ReservedSlotsList;

        #endregion

        #region Spectator List

        public virtual event SpectatorListConfigFileHandler SpectatorListConfigFile;
        public virtual event EmptyParamterHandler SpectatorListLoad;
        public virtual event EmptyParamterHandler SpectatorListSave;
        public virtual event SpectatorListPlayerHandler SpectatorListPlayerAdded;
        public virtual event SpectatorListPlayerHandler SpectatorListPlayerRemoved;
        public virtual event EmptyParamterHandler SpectatorListCleared;
        public virtual event SpectatorListListHandler SpectatorListList;

        #endregion

        #region Game Admin List

        public virtual event GameAdminConfigFileHandler GameAdminConfigFile;
        public virtual event EmptyParamterHandler GameAdminLoad;
        public virtual event EmptyParamterHandler GameAdminSave;
        public virtual event GameAdminPlayerHandler GameAdminPlayerAdded;
        public virtual event GameAdminPlayerHandler GameAdminPlayerRemoved;
        public virtual event EmptyParamterHandler GameAdminCleared;
        public virtual event GameAdminListHandler GameAdminList;

        #endregion

        #region Maplist

        public virtual event MapListConfigFileHandler MapListConfigFile;
        public virtual event EmptyParamterHandler MapListLoad;
        public virtual event EmptyParamterHandler MapListSave;
        public virtual event MapListAppendedHandler MapListMapAppended;
        public virtual event MapListLevelIndexHandler MapListNextLevelIndex;
        public virtual event MapListGetMapIndicesHandler MapListGetMapIndices;
        public virtual event MapListGetRoundsHandler MapListGetRounds;
        public virtual event MapListLevelIndexHandler MapListMapRemoved;
        public virtual event MapListMapInsertedHandler MapListMapInserted;
        public virtual event EmptyParamterHandler MapListCleared;
        public virtual event MapListListedHandler MapListListed;

        #endregion

        #region Variables (fired if setting or getting is successful)

        #region Configuration

        public virtual event PasswordHandler AdminPassword;
        public virtual event PasswordHandler GamePassword;
        public virtual event IsEnabledHandler Punkbuster;
        public virtual event IsEnabledHandler Ranked;
        public virtual event LimitHandler MaxPlayerLimit;
        public virtual event LimitHandler CurrentPlayerLimit;
        public virtual event LimitHandler PlayerLimit;
        public virtual event LimitHandler IdleTimeout;
        public virtual event LimitHandler IdleBanRounds;
        public virtual event IsEnabledHandler ProfanityFilter;

        #endregion

        #region Details

        public virtual event ServerNameHandler ServerName;
        public virtual event BannerUrlHandler BannerUrl;
        public virtual event ServerDescriptionHandler ServerDescription;
        public virtual event ServerMessageHandler ServerMessage;

        #endregion

        #region Gameplay

        public virtual event IsEnabledHandler Hardcore;
        public virtual event IsEnabledHandler FriendlyFire;

        #region BFBC2

        public virtual event LimitHandler RankLimit;
        public virtual event IsEnabledHandler KillCam;
        public virtual event IsEnabledHandler MiniMap;
        public virtual event IsEnabledHandler CrossHair;
        public virtual event IsEnabledHandler ThreeDSpotting;
        public virtual event IsEnabledHandler MiniMapSpotting;
        public virtual event IsEnabledHandler ThirdPersonVehicleCameras;
        public virtual event IsEnabledHandler TeamBalance;

        #endregion

        #region MoH

        public virtual event IsEnabledHandler ClanTeams;
        public virtual event UpperLowerLimitHandler SkillLimit;
        public virtual event UpperLowerLimitHandler PreRoundLimit;
        public virtual event IsEnabledHandler NoAmmoPickups;
        public virtual event IsEnabledHandler NoCrosshairs;
        public virtual event IsEnabledHandler NoSpotting;
        public virtual event IsEnabledHandler NoUnlocks;
        public virtual event IsEnabledHandler RealisticHealth;

        public virtual event EmptyParamterHandler StopPreRound;

        public virtual event IsEnabledHandler RoundStartTimer;
        public virtual event LimitHandler TdmScoreCounterMaxScore;
        public virtual event LimitHandler RoundStartTimerDelay;
        public virtual event LimitHandler RoundStartTimerPlayerLimit;

        #endregion

        #region BF3

        public virtual event IsEnabledHandler VehicleSpawnAllowed;
        public virtual event LimitHandler VehicleSpawnDelay;
        public virtual event LimitHandler BulletDamage;
        public virtual event IsEnabledHandler NameTag;
        public virtual event IsEnabledHandler RegenerateHealth;
        public virtual event IsEnabledHandler OnlySquadLeaderSpawn;
        public virtual event LimitHandler SoldierHealth;
        public virtual event IsEnabledHandler Hud;
        public virtual event LimitHandler PlayerManDownTime;
        public virtual event LimitHandler RoundRestartPlayerCount;
        public virtual event LimitHandler RoundStartPlayerCount;
        public virtual event LimitHandler PlayerRespawnTime;
        public virtual event LimitHandler GameModeCounter;
        public virtual event LimitHandler CtfRoundTimeModifier;
        public virtual event UnlockModeHandler UnlockMode;
        public virtual event GunMasterWeaponsPresetHandler GunMasterWeaponsPreset;
        public virtual event IsEnabledHandler ReservedSlotsListAggressiveJoin;
        public virtual event LimitHandler RoundLockdownCountdown;
        public virtual event LimitHandler RoundWarmupTimeout;
        public virtual event IsEnabledHandler PremiumStatus;

        #region player/squad cmd_handler

        public virtual event PlayerIdleStateHandler PlayerIdleState;
        public virtual event PlayerIsAliveHandler PlayerIsAlive;
        public virtual event PlayerPingedByAdminHandler PlayerPingedByAdmin;

        public virtual event SquadLeaderHandler SquadLeader;
        public virtual event SquadListActiveHandler SquadListActive;
        public virtual event SquadListPlayersHandler SquadListPlayers;
        public virtual event SquadIsPrivateHandler SquadIsPrivate;

        #endregion

        #endregion

        #region BF4

        public virtual event IsEnabledHandler FairFight;
        public virtual event LimitHandler MaxSpectators;
        public virtual event IsEnabledHandler IsHitIndicator;
        public virtual event IsEnabledHandler IsCommander;
        public virtual event IsEnabledHandler IsForceReloadWholeMags;
        public virtual event IsEnabledHandler AlwaysAllowSpectators;
        public virtual event VarsStringHandler ServerType;
        public virtual event LimitHandler RoundTimeLimit;
        public virtual event LimitHandler TicketBleedRate;
        public virtual event BF4presetHandler BF4preset;
        public virtual event MpExperienceHandler MpExperience;
        public virtual event LimitHandler RoundStartReadyPlayersNeeded;

        public virtual event TeamFactionOverrideHandler TeamFactionOverride;

        #endregion

        #region vars MoHW

        public virtual event IsEnabledHandler AllUnlocksUnlocked;
        public virtual event IsEnabledHandler BuddyOutline;
        public virtual event IsEnabledHandler HudBuddyInfo;
        public virtual event IsEnabledHandler HudClassAbility;
        public virtual event IsEnabledHandler HudCrosshair;
        public virtual event IsEnabledHandler HudEnemyTag;
        public virtual event IsEnabledHandler HudExplosiveIcons;
        public virtual event IsEnabledHandler HudGameMode;
        public virtual event IsEnabledHandler HudHealthAmmo;
        public virtual event IsEnabledHandler HudMinimap;
        public virtual event IsEnabledHandler HudObiturary;
        public virtual event IsEnabledHandler HudPointsTracker;
        public virtual event IsEnabledHandler HudUnlocks;
        public virtual event PlaylistSetHandler Playlist;

        #endregion

        #endregion

        #region Text Chat Moderation

        public virtual event TextChatModerationModeHandler TextChatModerationMode;
        public virtual event LimitHandler TextChatSpamTriggerCount;
        public virtual event LimitHandler TextChatSpamDetectionTime;
        public virtual event LimitHandler TextChatSpamCoolDownTime;

        #endregion

        #region Team Killing

        public virtual event LimitHandler TeamKillCountForKick;
        public virtual event LimitHandler TeamKillKickForBan;
        public virtual event LimitHandler TeamKillValueForKick;
        public virtual event LimitHandler TeamKillValueIncrease;
        public virtual event LimitHandler TeamKillValueDecreasePerSecond;

        #endregion

        #region Level Variables

        public virtual event LevelVariableHandler LevelVariablesSet;
        public virtual event LevelVariableHandler LevelVariablesClear;
        public virtual event LevelVariableGetHandler LevelVariablesGet;
        public virtual event LevelVariableGetHandler LevelVariablesEvaluate;
        public virtual event LevelVariableListHandler LevelVariablesList;

        #endregion

        #endregion

        #endregion

        #endregion
    }
}
