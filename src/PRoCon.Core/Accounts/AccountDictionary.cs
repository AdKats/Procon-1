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
using System.Collections.ObjectModel;

namespace PRoCon.Core.Accounts
{
    public class AccountDictionary : KeyedCollection<string, Account>
    {
        public delegate void AccountAlteredHandler(Account item);

        public event AccountAlteredHandler AccountAdded;
        public event AccountAlteredHandler AccountChanged;
        public event AccountAlteredHandler AccountRemoved;

        protected override string GetKeyForItem(Account item)
        {
            return item.Name;
        }

        protected override void InsertItem(int index, Account item)
        {
            base.InsertItem(index, item);

            if (AccountAdded != null)
            {
                this.AccountAdded(item);
            }
        }

        protected override void RemoveItem(int index)
        {
            if (AccountRemoved != null)
            {
                this.AccountRemoved(this[index]);
            }

            base.RemoveItem(index);
        }

        protected override void SetItem(int index, Account item)
        {
            if (AccountChanged != null)
            {
                this.AccountChanged(item);
            }

            base.SetItem(index, item);
        }

        public void CreateAccount(string strUsername, string strPassword)
        {
            if (Contains(strUsername) == true)
            {
                this[strUsername].Password = strPassword;
            }
            else
            {
                Add(new Account(strUsername, strPassword));
            }
        }

        public void DeleteAccount(string strUsername)
        {
            if (Contains(strUsername) == true)
            {
                Remove(strUsername);
            }
        }

        public void ChangePassword(string strUsername, string strPassword)
        {
            if (Contains(strUsername) == true)
            {
                this[strUsername].Password = strPassword;
            }
        }

        public List<string> ListAccountNames()
        {
            return new List<string>(Dictionary.Keys);
        }
    }
}