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

using System.Collections;
using System.Text.RegularExpressions;
using System.Windows.Forms;


namespace PRoCon.Controls.ControlsEx
{

    public class PlayerListColumnSorter : ListViewColumnSorter, IComparer
    {

        public Regex TotalsAveragesChecker
        {
            get;
            private set;
        }

        public PlayerListColumnSorter()
            : base()
        {
            this.TotalsAveragesChecker = new Regex("procon.playerlist.(totals|averages)[0-9]", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        public new int Compare(object x, object y)
        {
            ListViewItem listviewX = (ListViewItem)x, listviewY = (ListViewItem)y;

            if (this.TotalsAveragesChecker.IsMatch(listviewY.Name) == true)
            {
                return -1;
            }
            else if (this.TotalsAveragesChecker.IsMatch(listviewX.Name) == true)
            {
                return 1;
            }
            else
            {
                return base.Compare(x, y);
            }
        }

    }
}
