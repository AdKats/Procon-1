namespace PRoCon.Core.Remote.Layer.PacketDispatchers
{
    public class MohPacketDispatcher : LayerPacketDispatcher
    {
        public MohPacketDispatcher(ILayerConnection connection) : base(connection)
        {
            this.RequestDelegates.Add("vars.clanTeams", this.DispatchVarsRequest);
            this.RequestDelegates.Add("vars.noAmmoPickups", this.DispatchVarsRequest);
            this.RequestDelegates.Add("vars.noCrosshairs", this.DispatchVarsRequest);
            this.RequestDelegates.Add("vars.noSpotting", this.DispatchVarsRequest);
            this.RequestDelegates.Add("vars.noUnlocks", this.DispatchVarsRequest);
            this.RequestDelegates.Add("vars.realisticHealth", this.DispatchVarsRequest);
            this.RequestDelegates.Add("vars.skillLimit", this.DispatchVarsRequest);
            this.RequestDelegates.Add("vars.preRoundLimit", this.DispatchVarsRequest);

            this.RequestDelegates.Add("admin.stopPreRound", this.DispatchUseMapFunctionRequest);

            this.RequestDelegates.Add("admin.roundStartTimerEnabled", this.DispatchVarsRequest);
            this.RequestDelegates.Add("vars.tdmScoreCounterMaxScore", this.DispatchVarsRequest);
            this.RequestDelegates.Add("vars.roundStartTimerPlayersLimit", this.DispatchVarsRequest);
            this.RequestDelegates.Add("vars.roundStartTimerDelay", this.DispatchVarsRequest);

            this.RequestDelegates.Add("reservedSpectateSlots.configFile", this.DispatchAlterReservedSlotsListRequest);
            this.RequestDelegates.Add("reservedSpectateSlots.load", this.DispatchAlterReservedSlotsListRequest);
            this.RequestDelegates.Add("reservedSpectateSlots.save", this.DispatchAlterReservedSlotsListRequest);
            this.RequestDelegates.Add("reservedSpectateSlots.addPlayer", this.DispatchAlterReservedSlotsListRequest);
            this.RequestDelegates.Add("reservedSpectateSlots.removePlayer", this.DispatchAlterReservedSlotsListRequest);
            this.RequestDelegates.Add("reservedSpectateSlots.clear", this.DispatchAlterReservedSlotsListRequest);
            this.RequestDelegates.Add("reservedSpectateSlots.list", this.DispatchSecureSafeListedRequest);
        }

    }
}
