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

using System.Collections.Generic;

namespace PRoCon.Core.Lists
{
    using Core.Remote;

    public class ListsSettings
    {

        PRoConClient m_prcClient;

        public delegate void ManualBansVisibleChangeHandler(bool isVisible);
        public event ManualBansVisibleChangeHandler ManualBansVisibleChange;

        private bool m_isManualBansVisible;
        public bool ManualBansVisible
        {
            get
            {
                return this.m_isManualBansVisible;
            }
            set
            {
                this.m_isManualBansVisible = value;

                if (this.ManualBansVisibleChange != null)
                {
                    this.ManualBansVisibleChange(this.m_isManualBansVisible);
                }
            }
        }

        public string CurrentPlaylist
        {
            get;
            private set;
        }

        public List<string> Settings
        {
            get
            {
                return new List<string>() { this.m_isManualBansVisible.ToString() };
            }
            set
            {
                bool isVisible = true;

                if (value.Count >= 1 && bool.TryParse(value[0], out isVisible) == true)
                {
                    this.m_isManualBansVisible = isVisible;
                }
            }
        }

        public ListsSettings(PRoConClient prcClient)
        {
            this.m_isManualBansVisible = false;

            this.m_prcClient = prcClient;
            this.m_prcClient.Game.PlaylistSet += new FrostbiteClient.PlaylistSetHandler(m_prcClient_PlaylistSet);
        }

        private void m_prcClient_PlaylistSet(FrostbiteClient sender, string playlist)
        {
            this.CurrentPlaylist = playlist;
        }
    }
}
