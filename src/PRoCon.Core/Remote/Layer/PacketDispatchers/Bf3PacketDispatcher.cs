namespace PRoCon.Core.Remote.Layer.PacketDispatchers
{
    public class Bf3PacketDispatcher : LayerPacketDispatcher
    {
        public Bf3PacketDispatcher(ILayerConnection connection) : base(connection)
        {
            this.RequestDelegates.Add("admin.eventsEnabled", this.DispatchEventsEnabledRequest);

            // vars.idleTimeout is already included in FrostbiteLayerClient
            //this.m_requestDelegates.Add("vars.idleTimeout", this.DispatchVarsRequest);
            this.RequestDelegates.Add("vars.idleBanRounds", this.DispatchVarsRequest);
            this.RequestDelegates.Add("vars.maxPlayers", this.DispatchVarsRequest);
            this.RequestDelegates.Add("vars.3pCam", this.DispatchVarsRequest);
            this.RequestDelegates.Add("vars.vehicleSpawnAllowed", this.DispatchVarsRequest);
            this.RequestDelegates.Add("vars.vehicleSpawnDelay", this.DispatchVarsRequest);
            this.RequestDelegates.Add("vars.bulletDamage", this.DispatchVarsRequest);
            this.RequestDelegates.Add("vars.nameTag", this.DispatchVarsRequest);
            this.RequestDelegates.Add("vars.regenerateHealth", this.DispatchVarsRequest);
            this.RequestDelegates.Add("vars.roundRestartPlayerCount", this.DispatchVarsRequest);
            this.RequestDelegates.Add("vars.onlySquadLeaderSpawn", this.DispatchVarsRequest);
            this.RequestDelegates.Add("vars.unlockMode", this.DispatchVarsRequest);
            this.RequestDelegates.Add("vars.gunMasterWeaponsPreset", this.DispatchVarsRequest);
            this.RequestDelegates.Add("vars.soldierHealth", this.DispatchVarsRequest);
            this.RequestDelegates.Add("vars.hud", this.DispatchVarsRequest);
            this.RequestDelegates.Add("vars.playerManDownTime", this.DispatchVarsRequest);
            this.RequestDelegates.Add("vars.roundStartPlayerCount", this.DispatchVarsRequest);
            this.RequestDelegates.Add("vars.playerRespawnTime", this.DispatchVarsRequest);
            this.RequestDelegates.Add("vars.gameModeCounter", this.DispatchVarsRequest);
            this.RequestDelegates.Add("vars.ctfRoundTimeModifier", this.DispatchVarsRequest);
            this.RequestDelegates.Add("vars.roundLockdownCountdown", this.DispatchVarsRequest);
            this.RequestDelegates.Add("vars.roundWarmupTimeout", this.DispatchVarsRequest);

            this.RequestDelegates.Add("vars.killCam", this.DispatchVarsRequest);
            this.RequestDelegates.Add("vars.miniMap", this.DispatchVarsRequest);
            this.RequestDelegates.Add("vars.crossHair", this.DispatchVarsRequest);
            this.RequestDelegates.Add("vars.3dSpotting", this.DispatchVarsRequest);
            this.RequestDelegates.Add("vars.miniMapSpotting", this.DispatchVarsRequest);
            this.RequestDelegates.Add("vars.thirdPersonVehicleCameras", this.DispatchVarsRequest);
            this.RequestDelegates.Add("vars.autoBalance", this.DispatchVarsRequest);

            this.RequestDelegates.Add("reservedSlotsList.configFile", this.DispatchAlterReservedSlotsListRequest);
            this.RequestDelegates.Add("reservedSlotsList.load", this.DispatchAlterReservedSlotsListRequest);
            this.RequestDelegates.Add("reservedSlotsList.save", this.DispatchAlterReservedSlotsListRequest);
            this.RequestDelegates.Add("reservedSlotsList.add", this.DispatchAlterReservedSlotsListRequest);
            this.RequestDelegates.Add("reservedSlotsList.remove", this.DispatchAlterReservedSlotsListRequest);
            this.RequestDelegates.Add("reservedSlotsList.clear", this.DispatchAlterReservedSlotsListRequest);
            this.RequestDelegates.Add("reservedSlotsList.list", this.DispatchSecureSafeListedRequest);
            this.RequestDelegates.Add("reservedSlotsList.aggressiveJoin", this.DispatchVarsRequest);

            this.RequestDelegates.Add("currentLevel", this.DispatchSecureSafeListedRequest);

            this.RequestDelegates.Add("mapList.add", this.DispatchAlterMaplistRequest);

            this.RequestDelegates.Add("mapList.runNextRound", this.DispatchUseMapFunctionRequest);
            this.RequestDelegates.Add("mapList.restartRound", this.DispatchUseMapFunctionRequest);
            this.RequestDelegates.Add("mapList.endRound", this.DispatchUseMapFunctionRequest);
            this.RequestDelegates.Add("mapList.setNextMapIndex", this.DispatchUseMapFunctionRequest);
            this.RequestDelegates.Add("mapList.getMapIndices", this.DispatchSecureSafeListedRequest);
            this.RequestDelegates.Add("mapList.getRounds", this.DispatchUseMapFunctionRequest);

            this.RequestDelegates.Add("vars.serverMessage", this.DispatchVarsRequest);
            this.RequestDelegates.Add("vars.premiumStatus", this.DispatchVarsRequest);

            this.RequestDelegates.Add("player.idleDuration", this.DispatchSecureSafeListedRequest);
            this.RequestDelegates.Add("player.isAlive", this.DispatchSecureSafeListedRequest);
            this.RequestDelegates.Add("player.ping", this.DispatchSecureSafeListedRequest);
            this.RequestDelegates.Add("squad.leader", this.DispatchSquadLeaderRequest);
            this.RequestDelegates.Add("squad.listActive", this.DispatchSecureSafeListedRequest);
            this.RequestDelegates.Add("squad.listPlayers", this.DispatchSecureSafeListedRequest);
            this.RequestDelegates.Add("squad.private", this.DispatchSquadIsPrivateRequest);
        }
    }
}
