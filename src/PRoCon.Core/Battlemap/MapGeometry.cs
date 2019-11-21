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

using PRoCon.Core.Players;
using PRoCon.Core.Remote;
using System.Collections.Generic;

namespace PRoCon.Core.Battlemap
{
    public class MapGeometry
    {
        public delegate void MapZoneTrespassedHandler(CPlayerInfo soldier, ZoneAction action, MapZone sender, Point3D tresspassLocation, float tresspassPercentage, object trespassState);

        protected readonly PRoConClient Client;
        protected string CurrentMapFileName;

        public MapGeometry(PRoConClient prcClient)
        {
            MapZones = new MapZoneDictionary();

            if ((Client = prcClient) != null)
            {
                Client.Game.ServerInfo += new FrostbiteClient.ServerInfoHandler(m_prcClient_ServerInfo);
                Client.Game.LoadingLevel += new FrostbiteClient.LoadingLevelHandler(m_prcClient_LoadingLevel);
                Client.Game.LevelLoaded += new FrostbiteClient.LevelLoadedHandler(m_prcClient_LevelLoaded);
                Client.PlayerKilled += new PRoConClient.PlayerKilledHandler(m_prcClient_PlayerKilled);
            }
        }

        public MapZoneDictionary MapZones { get; private set; }
        public event MapZoneTrespassedHandler MapZoneTrespassed;

        private void m_prcClient_PlayerKilled(PRoConClient sender, Kill kKillerVictimDetails)
        {
            float trespassArea = 0.0F;

            foreach (MapZoneDrawing zone in new List<MapZoneDrawing>(MapZones))
            {
                if (System.String.Compare(CurrentMapFileName, zone.LevelFileName, System.StringComparison.OrdinalIgnoreCase) == 0)
                {
                    if ((trespassArea = zone.TrespassArea(kKillerVictimDetails.KillerLocation, 14.14F)) > 0.0F)
                    {
                        if (MapZoneTrespassed != null)
                        {
                            this.MapZoneTrespassed(kKillerVictimDetails.Killer, ZoneAction.Kill, new MapZone(zone.UID, zone.LevelFileName, zone.Tags.ToString(), zone.ZonePolygon, true), kKillerVictimDetails.KillerLocation, trespassArea, kKillerVictimDetails);
                        }
                    }

                    if ((trespassArea = zone.TrespassArea(kKillerVictimDetails.VictimLocation, 14.14F)) > 0.0F)
                    {
                        if (MapZoneTrespassed != null)
                        {
                            this.MapZoneTrespassed(kKillerVictimDetails.Victim, ZoneAction.Death, new MapZone(zone.UID, zone.LevelFileName, zone.Tags.ToString(), zone.ZonePolygon, true), kKillerVictimDetails.VictimLocation, trespassArea, kKillerVictimDetails);
                        }
                    }
                }
            }
        }

        private void m_prcClient_LoadingLevel(FrostbiteClient sender, string mapFileName, int roundsPlayed, int roundsTotal)
        {
            CurrentMapFileName = mapFileName;
        }

        private void m_prcClient_LevelLoaded(FrostbiteClient sender, string mapFileName, string gamemode, int roundsPlayed, int roundsTotal)
        {
            CurrentMapFileName = mapFileName;
        }

        private void m_prcClient_ServerInfo(FrostbiteClient sender, CServerInfo csiServerInfo)
        {
            CurrentMapFileName = csiServerInfo.Map;
        }
    }
}