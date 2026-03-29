using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using PRoCon.Core;
using PRoCon.Core.Remote;
using System;

namespace PRoCon.UI.Views
{
    public partial class ServerSettingsPanel : UserControl
    {
        private PRoConClient _client;
        private string _serverType = ""; // "OFFICIAL", "RANKED", "UNRANKED", "PRIVATE", or ""

        public ServerSettingsPanel()
        {
            InitializeComponent();
        }

        public void SetClient(PRoConClient client)
        {
            UnwireClient();
            _client = client;
            _serverType = "";

            if (_client?.Game == null)
            {
                IsEnabled = false;
                return;
            }

            IsEnabled = true;
            WireClient();
            RequestCurrentSettings();
        }

        public void SetApplication(PRoConApplication app)
        {
            // Not needed for server settings, but satisfies the interface contract.
        }

        // =====================================================================
        // Wire / Unwire events
        // =====================================================================

        private void WireClient()
        {
            if (_client?.Game == null) return;

            // General
            _client.Game.ServerType += OnServerType;
            _client.Game.BF4preset += OnPreset;
            _client.Game.ServerName += OnServerName;
            _client.Game.ServerDescription += OnServerDescription;
            _client.Game.ServerMessage += OnServerMessage;
            _client.Game.GamePassword += OnGamePassword;
            _client.Game.PlayerLimit += OnPlayerLimit;
            _client.Game.MaxSpectators += OnMaxSpectators;

            // Gameplay
            _client.Game.FriendlyFire += OnFriendlyFire;
            _client.Game.TeamBalance += OnAutoBalance;
            _client.Game.KillCam += OnKillCam;
            _client.Game.MiniMap += OnMiniMap;
            _client.Game.Hud += OnHud;
            _client.Game.ThreeDSpotting += OnThreeDSpotting;
            _client.Game.MiniMapSpotting += OnMiniMapSpotting;
            _client.Game.ThirdPersonVehicleCameras += OnThirdPersonCam;
            _client.Game.NameTag += OnNameTag;
            _client.Game.IsHitIndicator += OnHitIndicators;
            _client.Game.RegenerateHealth += OnRegenerateHealth;
            _client.Game.OnlySquadLeaderSpawn += OnOnlySquadLeaderSpawn;
            _client.Game.VehicleSpawnAllowed += OnVehicleSpawnAllowed;
            _client.Game.IsCommander += OnCommander;
            _client.Game.IsForceReloadWholeMags += OnForceReloadWholeMags;
            _client.Game.AlwaysAllowSpectators += OnAlwaysAllowSpectators;

            // Damage & Health
            _client.Game.BulletDamage += OnBulletDamage;
            _client.Game.SoldierHealth += OnSoldierHealth;
            _client.Game.PlayerRespawnTime += OnPlayerRespawnTime;
            _client.Game.VehicleSpawnDelay += OnVehicleSpawnDelay;

            // Tickets & Rounds
            _client.Game.GameModeCounter += OnGameModeCounter;
            _client.Game.TicketBleedRate += OnTicketBleedRate;
            _client.Game.RoundTimeLimit += OnRoundTimeLimit;
            _client.Game.RoundStartPlayerCount += OnRoundStartPlayerCount;
            _client.Game.RoundRestartPlayerCount += OnRoundRestartPlayerCount;
            _client.Game.RoundLockdownCountdown += OnRoundLockdownCountdown;
            _client.Game.RoundWarmupTimeout += OnRoundWarmupTimeout;

            // Team Kill
            _client.Game.TeamKillCountForKick += OnTKCountForKick;
            _client.Game.TeamKillKickForBan += OnTKKickForBan;
            _client.Game.TeamKillValueIncrease += OnTKValueIncrease;
            _client.Game.TeamKillValueDecreasePerSecond += OnTKValueDecrease;
            _client.Game.TeamKillValueForKick += OnTKValueForKick;

            // Idle
            _client.Game.IdleTimeout += OnIdleTimeout;
            _client.Game.IdleBanRounds += OnIdleBanRounds;

            // Misc
            _client.Game.UnlockMode += OnUnlockMode;
            _client.Game.GunMasterWeaponsPreset += OnGunMasterWeaponsPreset;
        }

        private void UnwireClient()
        {
            if (_client?.Game == null) return;

            _client.Game.ServerType -= OnServerType;
            _client.Game.BF4preset -= OnPreset;
            _client.Game.ServerName -= OnServerName;
            _client.Game.ServerDescription -= OnServerDescription;
            _client.Game.ServerMessage -= OnServerMessage;
            _client.Game.GamePassword -= OnGamePassword;
            _client.Game.PlayerLimit -= OnPlayerLimit;
            _client.Game.MaxSpectators -= OnMaxSpectators;

            _client.Game.FriendlyFire -= OnFriendlyFire;
            _client.Game.TeamBalance -= OnAutoBalance;
            _client.Game.KillCam -= OnKillCam;
            _client.Game.MiniMap -= OnMiniMap;
            _client.Game.Hud -= OnHud;
            _client.Game.ThreeDSpotting -= OnThreeDSpotting;
            _client.Game.MiniMapSpotting -= OnMiniMapSpotting;
            _client.Game.ThirdPersonVehicleCameras -= OnThirdPersonCam;
            _client.Game.NameTag -= OnNameTag;
            _client.Game.IsHitIndicator -= OnHitIndicators;
            _client.Game.RegenerateHealth -= OnRegenerateHealth;
            _client.Game.OnlySquadLeaderSpawn -= OnOnlySquadLeaderSpawn;
            _client.Game.VehicleSpawnAllowed -= OnVehicleSpawnAllowed;
            _client.Game.IsCommander -= OnCommander;
            _client.Game.IsForceReloadWholeMags -= OnForceReloadWholeMags;
            _client.Game.AlwaysAllowSpectators -= OnAlwaysAllowSpectators;

            _client.Game.BulletDamage -= OnBulletDamage;
            _client.Game.SoldierHealth -= OnSoldierHealth;
            _client.Game.PlayerRespawnTime -= OnPlayerRespawnTime;
            _client.Game.VehicleSpawnDelay -= OnVehicleSpawnDelay;

            _client.Game.GameModeCounter -= OnGameModeCounter;
            _client.Game.TicketBleedRate -= OnTicketBleedRate;
            _client.Game.RoundTimeLimit -= OnRoundTimeLimit;
            _client.Game.RoundStartPlayerCount -= OnRoundStartPlayerCount;
            _client.Game.RoundRestartPlayerCount -= OnRoundRestartPlayerCount;
            _client.Game.RoundLockdownCountdown -= OnRoundLockdownCountdown;
            _client.Game.RoundWarmupTimeout -= OnRoundWarmupTimeout;

            _client.Game.TeamKillCountForKick -= OnTKCountForKick;
            _client.Game.TeamKillKickForBan -= OnTKKickForBan;
            _client.Game.TeamKillValueIncrease -= OnTKValueIncrease;
            _client.Game.TeamKillValueDecreasePerSecond -= OnTKValueDecrease;
            _client.Game.TeamKillValueForKick -= OnTKValueForKick;

            _client.Game.IdleTimeout -= OnIdleTimeout;
            _client.Game.IdleBanRounds -= OnIdleBanRounds;

            _client.Game.UnlockMode -= OnUnlockMode;
            _client.Game.GunMasterWeaponsPreset -= OnGunMasterWeaponsPreset;
        }

        // =====================================================================
        // Request all current values
        // =====================================================================

        private void RequestCurrentSettings()
        {
            var game = _client?.Game;
            if (game == null) return;

            // Server type and preset first
            game.SendGetVarsServerType();
            game.SendGetVarsPresetPacket();

            // General
            game.SendGetVarsServerNamePacket();
            game.SendGetVarsServerDescriptionPacket();
            game.SendGetVarsServerMessagePacket();
            game.SendGetVarsGamePasswordPacket();
            game.SendGetVarsPlayerLimitPacket();
            game.SendGetVarsMaxSpectatorsPacket();

            // Gameplay
            game.SendGetVarsFriendlyFirePacket();
            game.SendGetVarsTeamBalancePacket();
            game.SendGetVarsKillCamPacket();
            game.SendGetVarsMiniMapPacket();
            game.SendGetVarsHudPacket();
            game.SendGetVars3dSpottingPacket();
            game.SendGetVarsMiniMapSpottingPacket();
            game.SendGetVarsThirdPersonVehicleCamerasPacket();
            game.SendGetVarsNameTagPacket();
            game.SendGetVarsHitIndicatorsEnabled();
            game.SendGetVarsRegenerateHealthPacket();
            game.SendGetVarsOnlySquadLeaderSpawnPacket();
            game.SendGetVarsVehicleSpawnAllowedPacket();
            game.SendGetVarsCommander();
            game.SendGetVarsForceReloadWholeMags();
            game.SendGetVarsAlwaysAllowSpectators();

            // Damage & Health
            game.SendGetVarsBulletDamagePacket();
            game.SendGetVarsSoldierHealthPacket();
            game.SendGetVarsPlayerRespawnTimePacket();
            game.SendGetVarsVehicleSpawnDelayPacket();

            // Tickets & Rounds
            game.SendGetVarsGameModeCounterPacket();
            game.SendGetVarsTicketBleedRatePacket();
            game.SendGetVarsRoundTimeLimitPacket();
            game.SendGetVarsRoundStartPlayerCountPacket();
            game.SendGetVarsRoundRestartPlayerCountPacket();
            game.SendGetVarsRoundLockdownCountdownPacket();
            game.SendGetVarsRoundWarmupTimeoutPacket();

            // Team Kill
            game.SendGetVarsTeamKillCountForKickPacket();
            game.SendGetVarsTeamKillKickForBanPacket();
            game.SendGetVarsTeamKillValueIncreasePacket();
            game.SendGetVarsTeamKillValueDecreasePerSecondPacket();
            game.SendGetVarsTeamKillValueForKickPacket();

            // Idle
            game.SendGetVarsIdleTimeoutPacket();
            game.SendGetVarsIdleBanRoundsPacket();

            // Misc
            game.SendGetVarsUnlockModePacket();
            game.SendGetVarsGunMasterWeaponsPresetPacket();
        }

        // =====================================================================
        // Server type handling
        // =====================================================================

        private void OnServerType(FrostbiteClient sender, string value)
        {
            Dispatcher.UIThread.Post(() =>
            {
                _serverType = (value ?? "").Trim().ToUpperInvariant();
                ApplyServerTypeRestrictions();
            });
        }

        private void OnPreset(FrostbiteClient sender, string mode, bool isLocked)
        {
            Dispatcher.UIThread.Post(() =>
            {
                var presetLabel = this.FindControl<TextBlock>("PresetLabel");
                if (presetLabel != null)
                    presetLabel.Text = string.IsNullOrEmpty(mode) ? "Unknown" : mode;
            });
        }

        private void ApplyServerTypeRestrictions()
        {
            var typeLabel = this.FindControl<TextBlock>("ServerTypeLabel");
            var typeHint = this.FindControl<TextBlock>("ServerTypeHint");

            if (typeLabel != null)
            {
                string displayType = _serverType switch
                {
                    "OFFICIAL" => "Official",
                    "RANKED" => "Ranked",
                    "UNRANKED" => "Unranked",
                    "PRIVATE" => "Private",
                    _ => string.IsNullOrEmpty(_serverType) ? "Unknown" : _serverType
                };
                typeLabel.Text = displayType;
            }

            if (_serverType == "OFFICIAL")
            {
                ApplyOfficialRestrictions();
                if (typeHint != null)
                    typeHint.Text = "Most settings are locked on Official servers.";
            }
            else if (_serverType == "RANKED")
            {
                ApplyRankedRestrictions();
                if (typeHint != null)
                    typeHint.Text = "Some settings have restricted ranges on Ranked servers.";
            }
            else
            {
                ApplyUnrankedRestrictions();
                if (typeHint != null)
                {
                    typeHint.Text = _serverType == "UNRANKED" || _serverType == "PRIVATE"
                        ? "All settings are editable."
                        : "";
                }
            }
        }

        private void ApplyOfficialRestrictions()
        {
            // Official: only server name and game password are editable

            // General
            SetControlEnabled("ServerNameInput", true);
            SetControlEnabled("ServerDescriptionInput", false);
            SetHintText("ServerDescriptionHint", "(locked on Official)");
            SetControlEnabled("ServerMessageInput", false);
            SetHintText("ServerMessageHint", "(locked on Official)");
            SetControlEnabled("GamePasswordInput", true);
            SetControlEnabled("MaxPlayersInput", false);
            SetHintText("MaxPlayersHint", "(locked on Official)");
            SetControlEnabled("MaxSpectatorsInput", false);
            SetHintText("MaxSpectatorsHint", "(locked on Official)");
            SetControlEnabled("ApplyGeneralButton", true);

            // Gameplay -- all read only
            string[] gameplayChecks = {
                "FriendlyFireCheck", "AutoBalanceCheck", "KillCamCheck", "MiniMapCheck",
                "HudCheck", "ThreeDSpottingCheck", "MiniMapSpottingCheck", "ThirdPersonCamCheck",
                "NameTagCheck", "HitIndicatorsCheck", "RegenerateHealthCheck",
                "OnlySquadLeaderSpawnCheck", "VehicleSpawnAllowedCheck", "CommanderCheck",
                "ForceReloadWholeMagsCheck", "IsNoobOnlyJoinCheck"
            };
            foreach (var name in gameplayChecks)
                SetControlEnabled(name, false);
            SetHintText("GameplayReadOnlyHint", "(locked on Official server)");

            // Damage & Health -- all read only
            SetControlEnabled("BulletDamageInput", false);
            SetControlEnabled("SoldierHealthInput", false);
            SetControlEnabled("PlayerRespawnTimeInput", false);
            SetControlEnabled("VehicleSpawnDelayInput", false);
            SetControlEnabled("ApplyDamageHealthButton", false);

            // Tickets & Rounds -- all read only
            string[] ticketInputs = {
                "GameModeCounterInput", "TicketBleedRateInput", "RoundTimeLimitInput",
                "RoundStartPlayerCountInput", "RoundRestartPlayerCountInput",
                "RoundLockdownCountdownInput", "RoundWarmupTimeoutInput",
                "RoundPlayersReadyBypassTimerInput", "RoundPlayersReadyMinCountInput",
                "RoundPlayersReadyPercentInput"
            };
            foreach (var name in ticketInputs)
                SetControlEnabled(name, false);
            SetControlEnabled("ApplyTicketsRoundsButton", false);

            // Team Kill -- all read only
            SetControlEnabled("TKCountForKickInput", false);
            SetControlEnabled("TKKickForBanInput", false);
            SetControlEnabled("TKValueIncreaseInput", false);
            SetControlEnabled("TKValueDecreaseInput", false);
            SetControlEnabled("TKValueForKickInput", false);
            SetControlEnabled("ApplyTKButton", false);

            // Idle -- all read only
            SetControlEnabled("IdleTimeoutInput", false);
            SetControlEnabled("IdleBanRoundsInput", false);
            SetControlEnabled("ApplyIdleButton", false);

            // Misc -- all read only
            SetControlEnabled("UnlockModeInput", false);
            SetControlEnabled("GunMasterWeaponsPresetInput", false);
            SetControlEnabled("ApplyMiscButton", false);
        }

        private void ApplyRankedRestrictions()
        {
            // Ranked: most settings editable with restricted ranges

            // General
            SetControlEnabled("ServerNameInput", true);
            SetHintText("ServerNameHint", "");
            SetControlEnabled("ServerDescriptionInput", true);
            SetHintText("ServerDescriptionHint", "");
            SetControlEnabled("ServerMessageInput", true);
            SetHintText("ServerMessageHint", "");
            SetControlEnabled("GamePasswordInput", true);
            SetControlEnabled("MaxPlayersInput", false);
            SetHintText("MaxPlayersHint", "(locked on Ranked)");
            SetControlEnabled("MaxSpectatorsInput", true);
            SetHintText("MaxSpectatorsHint", "");
            SetControlEnabled("ApplyGeneralButton", true);

            // Gameplay -- editable
            string[] gameplayChecks = {
                "FriendlyFireCheck", "AutoBalanceCheck", "KillCamCheck", "MiniMapCheck",
                "HudCheck", "ThreeDSpottingCheck", "MiniMapSpottingCheck", "ThirdPersonCamCheck",
                "NameTagCheck", "HitIndicatorsCheck", "RegenerateHealthCheck",
                "OnlySquadLeaderSpawnCheck", "VehicleSpawnAllowedCheck", "CommanderCheck",
                "ForceReloadWholeMagsCheck", "IsNoobOnlyJoinCheck"
            };
            foreach (var name in gameplayChecks)
                SetControlEnabled(name, true);
            SetHintText("GameplayReadOnlyHint", "");

            // Damage & Health -- editable with range hints
            SetControlEnabled("BulletDamageInput", true);
            SetControlEnabled("SoldierHealthInput", true);
            SetControlEnabled("PlayerRespawnTimeInput", true);
            SetControlEnabled("VehicleSpawnDelayInput", true);
            SetControlEnabled("ApplyDamageHealthButton", true);

            // Tickets & Rounds -- editable
            string[] ticketInputs = {
                "GameModeCounterInput", "TicketBleedRateInput", "RoundTimeLimitInput",
                "RoundStartPlayerCountInput", "RoundRestartPlayerCountInput",
                "RoundLockdownCountdownInput", "RoundWarmupTimeoutInput",
                "RoundPlayersReadyBypassTimerInput", "RoundPlayersReadyMinCountInput",
                "RoundPlayersReadyPercentInput"
            };
            foreach (var name in ticketInputs)
                SetControlEnabled(name, true);
            SetControlEnabled("ApplyTicketsRoundsButton", true);

            // Team Kill -- editable with range hints
            SetControlEnabled("TKCountForKickInput", true);
            SetHintText("TKCountForKickHint", "(Ranked: 4-10)");
            SetControlEnabled("TKKickForBanInput", true);
            SetControlEnabled("TKValueIncreaseInput", true);
            SetControlEnabled("TKValueDecreaseInput", true);
            SetControlEnabled("TKValueForKickInput", true);
            SetControlEnabled("ApplyTKButton", true);

            // Idle -- timeout is fixed at 300 on ranked
            SetControlEnabled("IdleTimeoutInput", false);
            SetHintText("IdleTimeoutHint", "(Ranked: fixed at 300)");
            SetControlEnabled("IdleBanRoundsInput", true);
            SetControlEnabled("ApplyIdleButton", true);

            // Misc -- editable
            SetControlEnabled("UnlockModeInput", true);
            SetControlEnabled("GunMasterWeaponsPresetInput", true);
            SetControlEnabled("ApplyMiscButton", true);
        }

        private void ApplyUnrankedRestrictions()
        {
            // Unranked / Private: everything editable

            // General
            SetControlEnabled("ServerNameInput", true);
            SetHintText("ServerNameHint", "");
            SetControlEnabled("ServerDescriptionInput", true);
            SetHintText("ServerDescriptionHint", "");
            SetControlEnabled("ServerMessageInput", true);
            SetHintText("ServerMessageHint", "");
            SetControlEnabled("GamePasswordInput", true);
            SetHintText("GamePasswordHint", "(empty to clear)");
            SetControlEnabled("MaxPlayersInput", true);
            SetHintText("MaxPlayersHint", "");
            SetControlEnabled("MaxSpectatorsInput", true);
            SetHintText("MaxSpectatorsHint", "");
            SetControlEnabled("ApplyGeneralButton", true);

            // Gameplay
            string[] gameplayChecks = {
                "FriendlyFireCheck", "AutoBalanceCheck", "KillCamCheck", "MiniMapCheck",
                "HudCheck", "ThreeDSpottingCheck", "MiniMapSpottingCheck", "ThirdPersonCamCheck",
                "NameTagCheck", "HitIndicatorsCheck", "RegenerateHealthCheck",
                "OnlySquadLeaderSpawnCheck", "VehicleSpawnAllowedCheck", "CommanderCheck",
                "ForceReloadWholeMagsCheck", "IsNoobOnlyJoinCheck"
            };
            foreach (var name in gameplayChecks)
                SetControlEnabled(name, true);
            SetHintText("GameplayReadOnlyHint", "");

            // Damage & Health
            SetControlEnabled("BulletDamageInput", true);
            SetControlEnabled("SoldierHealthInput", true);
            SetControlEnabled("PlayerRespawnTimeInput", true);
            SetControlEnabled("VehicleSpawnDelayInput", true);
            SetControlEnabled("ApplyDamageHealthButton", true);

            // Tickets & Rounds
            string[] ticketInputs = {
                "GameModeCounterInput", "TicketBleedRateInput", "RoundTimeLimitInput",
                "RoundStartPlayerCountInput", "RoundRestartPlayerCountInput",
                "RoundLockdownCountdownInput", "RoundWarmupTimeoutInput",
                "RoundPlayersReadyBypassTimerInput", "RoundPlayersReadyMinCountInput",
                "RoundPlayersReadyPercentInput"
            };
            foreach (var name in ticketInputs)
                SetControlEnabled(name, true);
            SetControlEnabled("ApplyTicketsRoundsButton", true);

            // Team Kill
            SetControlEnabled("TKCountForKickInput", true);
            SetHintText("TKCountForKickHint", "");
            SetControlEnabled("TKKickForBanInput", true);
            SetHintText("TKKickForBanHint", "");
            SetControlEnabled("TKValueIncreaseInput", true);
            SetHintText("TKValueIncreaseHint", "");
            SetControlEnabled("TKValueDecreaseInput", true);
            SetHintText("TKValueDecreaseHint", "");
            SetControlEnabled("TKValueForKickInput", true);
            SetHintText("TKValueForKickHint", "");
            SetControlEnabled("ApplyTKButton", true);

            // Idle
            SetControlEnabled("IdleTimeoutInput", true);
            SetHintText("IdleTimeoutHint", "(0 = disabled)");
            SetControlEnabled("IdleBanRoundsInput", true);
            SetHintText("IdleBanRoundsHint", "(0 = disabled)");
            SetControlEnabled("ApplyIdleButton", true);

            // Misc
            SetControlEnabled("UnlockModeInput", true);
            SetHintText("UnlockModeHint", "");
            SetControlEnabled("GunMasterWeaponsPresetInput", true);
            SetHintText("GunMasterWeaponsPresetHint", "");
            SetControlEnabled("ApplyMiscButton", true);
        }

        // =====================================================================
        // Event handlers for server responses
        // =====================================================================

        // --- General ---
        private void OnServerName(FrostbiteClient sender, string value)
        {
            Dispatcher.UIThread.Post(() => SetText("ServerNameInput", value));
        }

        private void OnServerDescription(FrostbiteClient sender, string value)
        {
            Dispatcher.UIThread.Post(() => SetText("ServerDescriptionInput", value));
        }

        private void OnServerMessage(FrostbiteClient sender, string value)
        {
            Dispatcher.UIThread.Post(() => SetText("ServerMessageInput", value));
        }

        private void OnGamePassword(FrostbiteClient sender, string value)
        {
            Dispatcher.UIThread.Post(() => SetText("GamePasswordInput", value));
        }

        private void OnPlayerLimit(FrostbiteClient sender, int value)
        {
            Dispatcher.UIThread.Post(() => SetText("MaxPlayersInput", value.ToString()));
        }

        private void OnMaxSpectators(FrostbiteClient sender, int value)
        {
            Dispatcher.UIThread.Post(() => SetText("MaxSpectatorsInput", value.ToString()));
        }

        // --- Gameplay ---
        private void OnFriendlyFire(FrostbiteClient sender, bool value)
        {
            Dispatcher.UIThread.Post(() => SetCheck("FriendlyFireCheck", value));
        }

        private void OnAutoBalance(FrostbiteClient sender, bool value)
        {
            Dispatcher.UIThread.Post(() => SetCheck("AutoBalanceCheck", value));
        }

        private void OnKillCam(FrostbiteClient sender, bool value)
        {
            Dispatcher.UIThread.Post(() => SetCheck("KillCamCheck", value));
        }

        private void OnMiniMap(FrostbiteClient sender, bool value)
        {
            Dispatcher.UIThread.Post(() => SetCheck("MiniMapCheck", value));
        }

        private void OnHud(FrostbiteClient sender, bool value)
        {
            Dispatcher.UIThread.Post(() => SetCheck("HudCheck", value));
        }

        private void OnThreeDSpotting(FrostbiteClient sender, bool value)
        {
            Dispatcher.UIThread.Post(() => SetCheck("ThreeDSpottingCheck", value));
        }

        private void OnMiniMapSpotting(FrostbiteClient sender, bool value)
        {
            Dispatcher.UIThread.Post(() => SetCheck("MiniMapSpottingCheck", value));
        }

        private void OnThirdPersonCam(FrostbiteClient sender, bool value)
        {
            Dispatcher.UIThread.Post(() => SetCheck("ThirdPersonCamCheck", value));
        }

        private void OnNameTag(FrostbiteClient sender, bool value)
        {
            Dispatcher.UIThread.Post(() => SetCheck("NameTagCheck", value));
        }

        private void OnHitIndicators(FrostbiteClient sender, bool value)
        {
            Dispatcher.UIThread.Post(() => SetCheck("HitIndicatorsCheck", value));
        }

        private void OnRegenerateHealth(FrostbiteClient sender, bool value)
        {
            Dispatcher.UIThread.Post(() => SetCheck("RegenerateHealthCheck", value));
        }

        private void OnOnlySquadLeaderSpawn(FrostbiteClient sender, bool value)
        {
            Dispatcher.UIThread.Post(() => SetCheck("OnlySquadLeaderSpawnCheck", value));
        }

        private void OnVehicleSpawnAllowed(FrostbiteClient sender, bool value)
        {
            Dispatcher.UIThread.Post(() => SetCheck("VehicleSpawnAllowedCheck", value));
        }

        private void OnCommander(FrostbiteClient sender, bool value)
        {
            Dispatcher.UIThread.Post(() => SetCheck("CommanderCheck", value));
        }

        private void OnForceReloadWholeMags(FrostbiteClient sender, bool value)
        {
            Dispatcher.UIThread.Post(() => SetCheck("ForceReloadWholeMagsCheck", value));
        }

        private void OnAlwaysAllowSpectators(FrostbiteClient sender, bool value)
        {
            Dispatcher.UIThread.Post(() => SetCheck("AlwaysAllowSpectatorsCheck", value));
        }

        // --- Damage & Health ---
        private void OnBulletDamage(FrostbiteClient sender, int value)
        {
            Dispatcher.UIThread.Post(() => SetText("BulletDamageInput", value.ToString()));
        }

        private void OnSoldierHealth(FrostbiteClient sender, int value)
        {
            Dispatcher.UIThread.Post(() => SetText("SoldierHealthInput", value.ToString()));
        }

        private void OnPlayerRespawnTime(FrostbiteClient sender, int value)
        {
            Dispatcher.UIThread.Post(() => SetText("PlayerRespawnTimeInput", value.ToString()));
        }

        private void OnVehicleSpawnDelay(FrostbiteClient sender, int value)
        {
            Dispatcher.UIThread.Post(() => SetText("VehicleSpawnDelayInput", value.ToString()));
        }

        // --- Tickets & Rounds ---
        private void OnGameModeCounter(FrostbiteClient sender, int value)
        {
            Dispatcher.UIThread.Post(() => SetText("GameModeCounterInput", value.ToString()));
        }

        private void OnTicketBleedRate(FrostbiteClient sender, int value)
        {
            Dispatcher.UIThread.Post(() => SetText("TicketBleedRateInput", value.ToString()));
        }

        private void OnRoundTimeLimit(FrostbiteClient sender, int value)
        {
            Dispatcher.UIThread.Post(() => SetText("RoundTimeLimitInput", value.ToString()));
        }

        private void OnRoundStartPlayerCount(FrostbiteClient sender, int value)
        {
            Dispatcher.UIThread.Post(() => SetText("RoundStartPlayerCountInput", value.ToString()));
        }

        private void OnRoundRestartPlayerCount(FrostbiteClient sender, int value)
        {
            Dispatcher.UIThread.Post(() => SetText("RoundRestartPlayerCountInput", value.ToString()));
        }

        private void OnRoundLockdownCountdown(FrostbiteClient sender, int value)
        {
            Dispatcher.UIThread.Post(() => SetText("RoundLockdownCountdownInput", value.ToString()));
        }

        private void OnRoundWarmupTimeout(FrostbiteClient sender, int value)
        {
            Dispatcher.UIThread.Post(() => SetText("RoundWarmupTimeoutInput", value.ToString()));
        }

        // --- Team Kill ---
        private void OnTKCountForKick(FrostbiteClient sender, int value)
        {
            Dispatcher.UIThread.Post(() => SetText("TKCountForKickInput", value.ToString()));
        }

        private void OnTKKickForBan(FrostbiteClient sender, int value)
        {
            Dispatcher.UIThread.Post(() => SetText("TKKickForBanInput", value.ToString()));
        }

        private void OnTKValueIncrease(FrostbiteClient sender, int value)
        {
            Dispatcher.UIThread.Post(() => SetText("TKValueIncreaseInput", value.ToString()));
        }

        private void OnTKValueDecrease(FrostbiteClient sender, int value)
        {
            Dispatcher.UIThread.Post(() => SetText("TKValueDecreaseInput", value.ToString()));
        }

        private void OnTKValueForKick(FrostbiteClient sender, int value)
        {
            Dispatcher.UIThread.Post(() => SetText("TKValueForKickInput", value.ToString()));
        }

        // --- Idle ---
        private void OnIdleTimeout(FrostbiteClient sender, int value)
        {
            Dispatcher.UIThread.Post(() => SetText("IdleTimeoutInput", value.ToString()));
        }

        private void OnIdleBanRounds(FrostbiteClient sender, int value)
        {
            Dispatcher.UIThread.Post(() => SetText("IdleBanRoundsInput", value.ToString()));
        }

        // --- Misc ---
        private void OnUnlockMode(FrostbiteClient sender, string value)
        {
            Dispatcher.UIThread.Post(() => SetText("UnlockModeInput", value));
        }

        private void OnGunMasterWeaponsPreset(FrostbiteClient sender, int value)
        {
            Dispatcher.UIThread.Post(() => SetText("GunMasterWeaponsPresetInput", value.ToString()));
        }

        // =====================================================================
        // Apply button handlers
        // =====================================================================

        private void OnApplyGeneral(object sender, RoutedEventArgs e)
        {
            var game = _client?.Game;
            if (game == null) return;

            string name = GetText("ServerNameInput");
            if (!string.IsNullOrEmpty(name))
                game.SendSetVarsServerNamePacket(name);

            if (_serverType != "OFFICIAL")
            {
                string desc = GetText("ServerDescriptionInput");
                if (desc != null)
                    game.SendSetVarsServerDescriptionPacket(desc);

                string msg = GetText("ServerMessageInput");
                if (msg != null)
                    game.SendSetVarsServerMessagePacket(msg);
            }

            string password = GetText("GamePasswordInput");
            if (password != null)
                game.SendSetVarsGamePasswordPacket(password);

            if (_serverType == "UNRANKED" || _serverType == "PRIVATE")
            {
                if (int.TryParse(GetText("MaxPlayersInput"), out int maxPlayers))
                    game.SendSetVarsPlayerLimitPacket(maxPlayers);
            }

            if (_serverType != "OFFICIAL")
            {
                if (int.TryParse(GetText("MaxSpectatorsInput"), out int maxSpectators))
                    game.SendSetVarsMaxSpectatorsPacket(maxSpectators);
            }

            SetStatus("General settings applied.");
        }

        private void OnGameplayToggle(object sender, RoutedEventArgs e)
        {
            var game = _client?.Game;
            if (game == null) return;

            if (sender is CheckBox cb)
            {
                if (_serverType == "OFFICIAL")
                {
                    SetStatus("Cannot change gameplay settings on an Official server.");
                    return;
                }

                bool isChecked = cb.IsChecked == true;
                string controlName = cb.Name;

                switch (controlName)
                {
                    case "FriendlyFireCheck":
                        game.SendSetVarsFriendlyFirePacket(isChecked);
                        break;
                    case "AutoBalanceCheck":
                        game.SendSetVarsTeamBalancePacket(isChecked);
                        break;
                    case "KillCamCheck":
                        game.SendSetVarsKillCamPacket(isChecked);
                        break;
                    case "MiniMapCheck":
                        game.SendSetVarsMiniMapPacket(isChecked);
                        break;
                    case "HudCheck":
                        game.SendSetVarsHudPacket(isChecked);
                        break;
                    case "ThreeDSpottingCheck":
                        game.SendSetVars3dSpottingPacket(isChecked);
                        break;
                    case "MiniMapSpottingCheck":
                        game.SendSetVarsMiniMapSpottingPacket(isChecked);
                        break;
                    case "ThirdPersonCamCheck":
                        game.SendSetVarsThirdPersonVehicleCamerasPacket(isChecked);
                        break;
                    case "NameTagCheck":
                        game.SendSetVarsNameTagPacket(isChecked);
                        break;
                    case "HitIndicatorsCheck":
                        game.SendSetVarsHitIndicatorsEnabled(isChecked);
                        break;
                    case "RegenerateHealthCheck":
                        game.SendSetVarsRegenerateHealthPacket(isChecked);
                        break;
                    case "OnlySquadLeaderSpawnCheck":
                        game.SendSetVarsOnlySquadLeaderSpawnPacket(isChecked);
                        break;
                    case "VehicleSpawnAllowedCheck":
                        game.SendSetVarsVehicleSpawnAllowedPacket(isChecked);
                        break;
                    case "CommanderCheck":
                        game.SendSetVarsCommander(isChecked);
                        break;
                    case "ForceReloadWholeMagsCheck":
                        game.SendSetVarsForceReloadWholeMags(isChecked);
                        break;
                    case "IsNoobOnlyJoinCheck":
                        // No dedicated method; use same pattern but skip if unavailable
                        break;
                }

                SetStatus($"{cb.Content} set to {isChecked}.");
            }
        }

        private void OnApplyDamageHealth(object sender, RoutedEventArgs e)
        {
            var game = _client?.Game;
            if (game == null) return;

            if (_serverType == "OFFICIAL")
            {
                SetStatus("Cannot change damage/health settings on an Official server.");
                return;
            }

            if (int.TryParse(GetText("BulletDamageInput"), out int bulletDamage))
                game.SendSetVarsBulletDamagePacket(bulletDamage);

            if (int.TryParse(GetText("SoldierHealthInput"), out int soldierHealth))
                game.SendSetVarsSoldierHealthPacket(soldierHealth);

            if (int.TryParse(GetText("PlayerRespawnTimeInput"), out int respawnTime))
                game.SendSetVarsPlayerRespawnTimePacket(respawnTime);

            if (int.TryParse(GetText("VehicleSpawnDelayInput"), out int vehicleDelay))
                game.SendSetVarsVehicleSpawnDelayPacket(vehicleDelay);

            SetStatus("Damage / Health settings applied.");
        }

        private void OnApplyTicketsRounds(object sender, RoutedEventArgs e)
        {
            var game = _client?.Game;
            if (game == null) return;

            if (_serverType == "OFFICIAL")
            {
                SetStatus("Cannot change ticket/round settings on an Official server.");
                return;
            }

            if (int.TryParse(GetText("GameModeCounterInput"), out int gameModeCounter))
                game.SendSetVarsGameModeCounterPacket(gameModeCounter);

            if (int.TryParse(GetText("TicketBleedRateInput"), out int ticketBleedRate))
                game.SendSetVarsTicketBleedRatePacket(ticketBleedRate);

            if (int.TryParse(GetText("RoundTimeLimitInput"), out int roundTimeLimit))
                game.SendSetVarsRoundTimeLimitPacket(roundTimeLimit);

            if (int.TryParse(GetText("RoundStartPlayerCountInput"), out int roundStartPlayerCount))
                game.SendSetVarsRoundStartPlayerCountPacket(roundStartPlayerCount);

            if (int.TryParse(GetText("RoundRestartPlayerCountInput"), out int roundRestartPlayerCount))
                game.SendSetVarsRoundRestartPlayerCountPacket(roundRestartPlayerCount);

            if (int.TryParse(GetText("RoundLockdownCountdownInput"), out int lockdown))
                game.SendSetVarsRoundLockdownCountdownPacket(lockdown);

            if (int.TryParse(GetText("RoundWarmupTimeoutInput"), out int warmup))
                game.SendSetVarsRoundWarmupTimeoutPacket(warmup);

            // roundPlayersReadyBypassTimer, roundPlayersReadyMinCount, roundPlayersReadyPercent
            // do not have dedicated Send methods -- values shown read-only from RCON responses

            SetStatus("Tickets / Rounds settings applied.");
        }

        private void OnApplyTeamKill(object sender, RoutedEventArgs e)
        {
            var game = _client?.Game;
            if (game == null) return;

            if (_serverType == "OFFICIAL")
            {
                SetStatus("Cannot change team kill settings on an Official server.");
                return;
            }

            if (int.TryParse(GetText("TKCountForKickInput"), out int tkCount))
            {
                if (_serverType == "RANKED")
                    tkCount = Math.Clamp(tkCount, 4, 10);
                game.SendSetVarsTeamKillCountForKickPacket(tkCount);
            }

            if (int.TryParse(GetText("TKKickForBanInput"), out int tkKickForBan))
                game.SendSetVarsTeamKillKickForBanPacket(tkKickForBan);

            if (int.TryParse(GetText("TKValueIncreaseInput"), out int tkIncrease))
                game.SendSetVarsTeamKillValueIncreasePacket(tkIncrease);

            if (int.TryParse(GetText("TKValueDecreaseInput"), out int tkDecrease))
                game.SendSetVarsTeamKillValueDecreasePerSecondPacket(tkDecrease);

            if (int.TryParse(GetText("TKValueForKickInput"), out int tkValueForKick))
                game.SendSetVarsTeamKillValueForKickPacket(tkValueForKick);

            SetStatus("Team kill settings applied.");
        }

        private void OnApplyIdle(object sender, RoutedEventArgs e)
        {
            var game = _client?.Game;
            if (game == null) return;

            if (_serverType == "OFFICIAL")
            {
                SetStatus("Cannot change idle settings on an Official server.");
                return;
            }

            if (_serverType != "RANKED")
            {
                if (int.TryParse(GetText("IdleTimeoutInput"), out int timeout))
                    game.SendSetVarsIdleTimeoutPacket(timeout);
            }

            if (int.TryParse(GetText("IdleBanRoundsInput"), out int banRounds))
                game.SendSetVarsIdleBanRoundsPacket(banRounds);

            SetStatus("Idle settings applied.");
        }

        private void OnApplyMisc(object sender, RoutedEventArgs e)
        {
            var game = _client?.Game;
            if (game == null) return;

            if (_serverType == "OFFICIAL")
            {
                SetStatus("Cannot change misc settings on an Official server.");
                return;
            }

            string unlockMode = GetText("UnlockModeInput");
            if (!string.IsNullOrEmpty(unlockMode))
                game.SendSetVarsUnlockModePacket(unlockMode);

            if (int.TryParse(GetText("GunMasterWeaponsPresetInput"), out int preset))
            {
                preset = Math.Clamp(preset, 0, 4);
                game.SendSetVarsGunMasterWeaponsPresetPacket(preset);
            }

            SetStatus("Misc settings applied.");
        }

        // =====================================================================
        // Helpers
        // =====================================================================

        private void SetText(string controlName, string value)
        {
            var tb = this.FindControl<TextBox>(controlName);
            if (tb != null) tb.Text = value ?? "";
        }

        private string GetText(string controlName)
        {
            var tb = this.FindControl<TextBox>(controlName);
            return tb?.Text ?? "";
        }

        private void SetCheck(string controlName, bool value)
        {
            var cb = this.FindControl<CheckBox>(controlName);
            if (cb != null) cb.IsChecked = value;
        }

        private void SetStatus(string message)
        {
            var status = this.FindControl<TextBlock>("SettingsStatusText");
            if (status != null) status.Text = message;
        }

        private void SetControlEnabled(string controlName, bool enabled)
        {
            var control = this.FindControl<Control>(controlName);
            if (control != null)
            {
                control.IsEnabled = enabled;
                if (control is TextBox tb)
                    tb.Opacity = enabled ? 1.0 : 0.5;
                else if (control is CheckBox cb)
                    cb.Opacity = enabled ? 1.0 : 0.5;
                else if (control is Button btn)
                    btn.Opacity = enabled ? 1.0 : 0.5;
            }
        }

        private void SetHintText(string controlName, string text)
        {
            var label = this.FindControl<TextBlock>(controlName);
            if (label != null) label.Text = text;
        }
    }
}
