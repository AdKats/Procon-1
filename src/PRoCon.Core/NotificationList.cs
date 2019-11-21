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
using System.Collections.ObjectModel;
using System.Windows.Forms;

namespace PRoCon.Core
{

    public class NotificationList<T> : Collection<T>
    {

        public delegate void ItemModifiedHandler(int iIndex, T item);
        public event ItemModifiedHandler ItemAdded;
        public event ItemModifiedHandler ItemChanged;
        public event ItemModifiedHandler ItemRemoved;

        protected void RaiseEvent(Delegate[] a_delInvokes, params object[] a_objArguments)
        {
            for (int i = 0; i < a_delInvokes.Length; i++)
            {
                if (a_delInvokes[i].Target is Control)
                {
                    ((Control)a_delInvokes[i].Target).Invoke(a_delInvokes[i], a_objArguments);
                }
                else
                {

                    a_delInvokes[i].DynamicInvoke(a_objArguments);
                }
            }
        }

        protected override void InsertItem(int index, T newItem)
        {
            base.InsertItem(index, newItem);

            if (this.ItemAdded != null)
            {
                this.RaiseEvent(this.ItemAdded.GetInvocationList(), index, newItem);
            }
        }

        protected override void SetItem(int index, T newItem)
        {
            if (this.ItemChanged != null)
            {
                this.RaiseEvent(this.ItemChanged.GetInvocationList(), index, newItem);
            }

            base.SetItem(index, newItem);
        }

        protected override void RemoveItem(int index)
        {
            if (this.ItemRemoved != null)
            {
                this.RaiseEvent(this.ItemRemoved.GetInvocationList(), index, this.Items[index]);
            }

            base.RemoveItem(index);
        }

        public T Find(Predicate<T> pred)
        {
            T tReturn = default(T);

            foreach (T t in this)
            {
                if (pred(t) == true)
                {
                    tReturn = t;
                    break;
                }
            }

            return tReturn;
        }

        public List<T> FindAll(Predicate<T> pred)
        {
            List<T> tReturn = new List<T>();

            foreach (T t in this)
            {
                if (pred(t) == true)
                {
                    tReturn.Add(t);
                }
            }

            return tReturn;
        }

    }
}