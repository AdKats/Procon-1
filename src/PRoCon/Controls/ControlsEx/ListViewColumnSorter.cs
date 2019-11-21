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
using System.Collections;
using System.Windows.Forms;

namespace PRoCon.Controls.ControlsEx
{

    // MSDN
    public class ListViewColumnSorter : IComparer
    {
        private int ColumnToSort;
        private SortOrder OrderOfSort;
        private CaseInsensitiveComparer ObjectCompare;

        public ListViewColumnSorter()
        {
            ColumnToSort = 0;
            OrderOfSort = SortOrder.None;
            ObjectCompare = new CaseInsensitiveComparer();
        }

        public int Compare(object x, object y)
        {
            int compareResult;
            ListViewItem listviewX = (ListViewItem)x, listviewY = (ListViewItem)y;

            UInt32 uix = 0, uiy = 0;
            Double dblx = 0, dbly = 0;

            if (Double.TryParse(listviewX.SubItems[ColumnToSort].Text, out dblx) == true && Double.TryParse(listviewY.SubItems[ColumnToSort].Text, out dbly) == true)
            {
                // Comparing two ints
                compareResult = ObjectCompare.Compare(dblx, dbly);
            }
            else
            {
                if (listviewX.SubItems[ColumnToSort].Tag != null && listviewY.SubItems[ColumnToSort].Tag != null
                    && listviewX.SubItems[ColumnToSort].Tag is UInt32 && listviewY.SubItems[ColumnToSort].Tag is UInt32
                    && UInt32.TryParse(((UInt32)listviewX.SubItems[ColumnToSort].Tag).ToString(), out uix) == true && UInt32.TryParse(((UInt32)listviewY.SubItems[ColumnToSort].Tag).ToString(), out uiy) == true)
                {
                    // Comparing two times.
                    compareResult = ObjectCompare.Compare(uix, uiy);
                }
                else
                {
                    // Comparing two strings.
                    compareResult = ObjectCompare.Compare(listviewX.SubItems[ColumnToSort].Text, listviewY.SubItems[ColumnToSort].Text);
                }
            }

            //if (Regex.Match(listviewY.Name, "procon.playerlist.totals[0-9]", RegexOptions.IgnoreCase).Success == true) {
            //    return -1;
            //}
            //else if (Regex.Match(listviewY.Name, "procon.playerlist.totals[0-9]", RegexOptions.IgnoreCase).Success == false) {
            //    return 1;
            //}
            //else {
            if (OrderOfSort == SortOrder.Ascending)
            {
                return compareResult;
            }
            else if (OrderOfSort == SortOrder.Descending)
            {
                return (-compareResult);
            }
            else
            {
                return 0;
            }
            //}
        }

        public int SortColumn
        {
            set { ColumnToSort = value; }
            get { return ColumnToSort; }
        }

        public SortOrder Order
        {
            set { OrderOfSort = value; }
            get { return OrderOfSort; }
        }
    }
}
