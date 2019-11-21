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

using System;

namespace PRoCon.Core
{
    using Core.Players;
    [Serializable]
    public class Kill
    {

        public CPlayerInfo Killer
        {
            get;
            private set;
        }

        public CPlayerInfo Victim
        {
            get;
            private set;
        }

        /*
        public string Killer {
            get;
            private set;
        }
        */

        public Point3D KillerLocation
        {
            get;
            private set;
        }

        /*
        public string Victim {
            get;
            private set;
        }
        */

        public Point3D VictimLocation
        {
            get;
            private set;
        }

        public string DamageType
        {
            get;
            private set;
        }

        public bool Headshot
        {
            get;
            private set;
        }

        public bool IsSuicide
        {
            get;
            private set;
        }

        // TO DO: Change set back to private only.
        public DateTime TimeOfDeath
        {
            get;
            set;
        }

        public double Distance
        {
            get;
            private set;
        }

        public Kill(CPlayerInfo cpKiller, CPlayerInfo cpVictim, string strDamageType, bool blHeadshot, Point3D pntKiller, Point3D pntVictim)
        {

            if ((this.Killer = cpKiller) == null)
            {
                this.Killer = new CPlayerInfo();
            }

            if ((this.Victim = cpVictim) == null)
            {
                this.Victim = new CPlayerInfo();
            }

            this.IsSuicide = (String.Compare(this.Killer.SoldierName, this.Victim.SoldierName) == 0);

            this.DamageType = strDamageType;
            this.Headshot = blHeadshot;
            this.KillerLocation = pntKiller;
            this.VictimLocation = pntVictim;

            this.TimeOfDeath = DateTime.Now;

            double dx = pntKiller.X - pntVictim.X;
            double dy = pntKiller.Y - pntVictim.Y;
            double dz = pntKiller.Z - pntVictim.Z;

            this.Distance = Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        /*
        public Kill(string strKiller, string strVictim, string strDamageType, bool blHeadshot, Point3D pntKiller, Point3D pntVictim) {
            this.Killer = strKiller;
            this.Victim = strVictim;
            this.DamageType = strDamageType;
            this.Headshot = blHeadshot;
            this.KillerLocation = pntKiller;
            this.VictimLocation = pntVictim;

            this.TimeOfDeath = DateTime.Now;

            double dx = pntKiller.X - pntVictim.X;
            double dy = pntKiller.Y - pntVictim.Y;
            double dz = pntKiller.Z - pntVictim.Z;

            this.Distance = Math.Sqrt(dx*dx + dy*dy + dz*dz);
        }
        */
    }
}
