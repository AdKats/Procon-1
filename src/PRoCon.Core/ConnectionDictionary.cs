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

using System.Collections;
using System.Collections.ObjectModel;

namespace PRoCon
{
    using Core;
    using Core.Remote;
    public class ConnectionDictionary : KeyedCollection<string, PRoConClient>
    {

        public delegate void ConnectionAlteredHandler(PRoConClient item);
        public event ConnectionAlteredHandler ConnectionAdded;
        public event ConnectionAlteredHandler ConnectionChanged;
        public event ConnectionAlteredHandler ConnectionRemoved;

        protected override string GetKeyForItem(PRoConClient item)
        {
            return item.HostNamePort;
        }

        protected override void InsertItem(int index, PRoConClient item)
        {
            if (this.ConnectionAdded != null)
            {
                this.ConnectionAdded(item);
            }

            base.InsertItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            PRoConClient prcClient = this[index];

            base.RemoveItem(index);

            if (this.ConnectionRemoved != null)
            {
                this.ConnectionRemoved(prcClient);
            }
        }

        protected override void SetItem(int index, PRoConClient item)
        {
            if (this.ConnectionChanged != null)
            {
                this.ConnectionChanged(item);
            }

            base.SetItem(index, item);
        }

        public string ToJsonString()
        {

            Hashtable connections = new Hashtable();

            ArrayList connectionList = new ArrayList();
            foreach (PRoConClient client in this)
            {

                Hashtable connection = new Hashtable();

                connection.Add("host_name_port", client.HostNamePort);
                connection.Add("connected", (bool)(client.State == ConnectionState.Connected));
                connection.Add("logged_in", client.IsLoggedIn);
                connection.Add("procon_connection", client.IsPRoConConnection);
                connection.Add("gametype", client.GameType);
                connection.Add("version", client.VersionNumber);

                connectionList.Add(connection);
            }

            connections.Add("connections", connectionList);

            return JSON.JsonEncode(connections);
        }
    }
}
