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

using System;
using System.Collections.Generic;

namespace PRoCon.Core
{
    [Serializable]
    public class Point3D
    {
        public int X
        {
            get;
            set;
        }

        public int Y
        {
            get;
            set;
        }

        public int Z
        {
            get;
            set;
        }

        public Point3D()
        {
            this.X = 0;
            this.Y = 0;
            this.Z = 0;
        }

        public Point3D(int iX, int iY, int iZ)
        {
            this.X = iX;
            this.Y = iY;
            this.Z = iZ;
        }

        public Point3D(string strX, string strY, string strZ)
        {
            int iX = 0, iY = 0, iZ = 0;

            int.TryParse(strX, out iX);
            int.TryParse(strY, out iY);
            int.TryParse(strZ, out iZ);

            this.X = iX;
            this.Y = iY;
            this.Z = iZ;
        }

        public static List<string> ToStringList(Point3D[] points)
        {

            List<string> list = new List<string>();

            foreach (Point3D point in points)
            {
                list.Add(point.X.ToString());
                list.Add(point.Y.ToString());
                list.Add(point.Z.ToString());
            }

            return list;
        }

    }
}
