namespace PRoCon.Core.Remote.Layer.PacketDispatchers
{
    public class MohwPacketDispatcher : LayerPacketDispatcher
    {
        public MohwPacketDispatcher(ILayerConnection connection) : base(connection)
        {
            this.RequestDelegates.Add("admin.eventsEnabled", this.DispatchEventsEnabledRequest);

            #region FrostbiteLayerClient delegates
            // vars.bannerUrl -> FrostbiteLayerClient
            // vars.friendlyFire -> FrostbiteLayerClient
            // vars.gamePassword -> FrostbiteLayerClient
            // vars.idleTimeout -> FrostbiteLayerClient
            // vars.ranked -> FrostbiteLayerClient
            // vars.serverDescription
            // vars.serverName
            // vars.teamKill*
            #endregion

            #region MoHW R-3 disabled
            // MoHW R-3 this.m_requestDelegates.Add("reservedSlotsList.aggressiveJoin", this.DispatchVarsRequest);

            // MoHW R-3 this.m_requestDelegates.Add("vars.3dSpotting", this.DispatchVarsRequest);
            // MoHW R-3 this.m_requestDelegates.Add("vars.hud", this.DispatchVarsRequest);
            // MoHW R-3 this.m_requestDelegates.Add("vars.miniMap", this.DispatchVarsRequest);
            // this.m_requestDelegates.Add("vars.miniMapSpotting", this.DispatchVarsRequest);
            // MoHW R-3 this.m_requestDelegates.Add("vars.nameTag", this.DispatchVarsRequest);
            // MoHW R-3 this.m_requestDelegates.Add("vars.onlySquadLeaderSpawn", this.DispatchVarsRequest);
            // this.m_requestDelegates.Add("vars.thirdPersonVehicleCameras", this.DispatchVarsRequest);
            // MOHW general this.m_requestDelegates.Add("vars.premiumStatus", this.DispatchVarsRequest);
            // MoHW R-3 this.m_requestDelegates.Add("vars.roundLockdownCountdown", this.DispatchVarsRequest);
            // MoHW R-3 this.m_requestDelegates.Add("vars.roundWarmupTimeout", this.DispatchVarsRequest);
            // MoHW R-3 this.m_requestDelegates.Add("vars.unlockMode", this.DispatchVarsRequest);
            // MoHW R-3 this.m_requestDelegates.Add("vars.vehicleSpawnAllowed", this.DispatchVarsRequest);
            // MoHW R-3 this.m_requestDelegates.Add("vars.vehicleSpawnDelay", this.DispatchVarsRequest);
            #endregion

            this.RequestDelegates.Add("vars.3pCam", this.DispatchVarsRequest);
            // R-5 this.m_requestDelegates.Add("vars.allUnlocksUnlocked", this.DispatchVarsRequest); // MoHW
            this.RequestDelegates.Add("vars.autoBalance", this.DispatchVarsRequest);
            this.RequestDelegates.Add("vars.buddyOutline", this.DispatchVarsRequest); // MoHW
            this.RequestDelegates.Add("vars.bulletDamage", this.DispatchVarsRequest);
            this.RequestDelegates.Add("vars.gameModeCounter", this.DispatchVarsRequest);

            this.RequestDelegates.Add("vars.hudBuddyInfo", this.DispatchVarsRequest); // MoHW
            this.RequestDelegates.Add("vars.hudClassAbility", this.DispatchVarsRequest); // MoHW
            this.RequestDelegates.Add("vars.hudCrosshair", this.DispatchVarsRequest); // MoHW
            this.RequestDelegates.Add("vars.hudEnemyTag", this.DispatchVarsRequest); // MoHW
            this.RequestDelegates.Add("vars.hudExplosiveIcons", this.DispatchVarsRequest); // MoHW
            this.RequestDelegates.Add("vars.hudGameMode", this.DispatchVarsRequest); // MoHW
            this.RequestDelegates.Add("vars.hudHealthAmmo", this.DispatchVarsRequest); // MoHW
            this.RequestDelegates.Add("vars.hudMinimap", this.DispatchVarsRequest); // MoHW
            this.RequestDelegates.Add("vars.hudObiturary", this.DispatchVarsRequest); // MoHW
            this.RequestDelegates.Add("vars.hudPointsTracker", this.DispatchVarsRequest); // MoHW
            this.RequestDelegates.Add("vars.hudUnlocks", this.DispatchVarsRequest); // MoHW

            this.RequestDelegates.Add("vars.idleBanRounds", this.DispatchVarsRequest);
            this.RequestDelegates.Add("vars.killCam", this.DispatchVarsRequest);
            this.RequestDelegates.Add("vars.maxPlayers", this.DispatchVarsRequest);
            this.RequestDelegates.Add("vars.playlist", this.DispatchAlterMaplistRequest); // MoHW
            this.RequestDelegates.Add("vars.playerManDownTime", this.DispatchVarsRequest);
            this.RequestDelegates.Add("vars.playerRespawnTime", this.DispatchVarsRequest);
            this.RequestDelegates.Add("vars.regenerateHealth", this.DispatchVarsRequest);
            this.RequestDelegates.Add("vars.roundRestartPlayerCount", this.DispatchVarsRequest);
            this.RequestDelegates.Add("vars.roundStartPlayerCount", this.DispatchVarsRequest);
            this.RequestDelegates.Add("vars.soldierHealth", this.DispatchVarsRequest);

            this.RequestDelegates.Add("reservedSlotsList.configFile", this.DispatchAlterReservedSlotsListRequest);
            this.RequestDelegates.Add("reservedSlotsList.load", this.DispatchAlterReservedSlotsListRequest);
            this.RequestDelegates.Add("reservedSlotsList.save", this.DispatchAlterReservedSlotsListRequest);
            this.RequestDelegates.Add("reservedSlotsList.add", this.DispatchAlterReservedSlotsListRequest);
            this.RequestDelegates.Add("reservedSlotsList.remove", this.DispatchAlterReservedSlotsListRequest);
            this.RequestDelegates.Add("reservedSlotsList.clear", this.DispatchAlterReservedSlotsListRequest);
            this.RequestDelegates.Add("reservedSlotsList.list", this.DispatchSecureSafeListedRequest);

            this.RequestDelegates.Add("currentLevel", this.DispatchSecureSafeListedRequest);

            this.RequestDelegates.Add("mapList.add", this.DispatchAlterMaplistRequest);

            this.RequestDelegates.Add("mapList.runNextRound", this.DispatchUseMapFunctionRequest);
            this.RequestDelegates.Add("mapList.restartRound", this.DispatchUseMapFunctionRequest);
            this.RequestDelegates.Add("mapList.endRound", this.DispatchUseMapFunctionRequest);
            this.RequestDelegates.Add("mapList.setNextMapIndex", this.DispatchUseMapFunctionRequest);
            this.RequestDelegates.Add("mapList.getMapIndices", this.DispatchSecureSafeListedRequest);
            this.RequestDelegates.Add("mapList.getRounds", this.DispatchUseMapFunctionRequest);

            this.RequestDelegates.Add("vars.serverMessage", this.DispatchVarsRequest);
        }
    }
}
