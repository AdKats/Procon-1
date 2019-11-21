using PRoCon.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PRoCon.Controls.Data
{
    public class BansSource : ISource
    {
        private string _filter;
        private int _skip;
        private int _take;

        /// <summary>
        /// The items stored for this source
        /// </summary>
        public List<CBanInfo> Items { get; set; }

        /// <summary>
        /// When the items were last set. We use this to ignore full refreshes if one was done
        /// in the last minute. Prevents redrawing information we already know about.
        /// </summary>
        protected DateTime ItemsAge { get; set; }

        /// <summary>
        /// The items that have been filtered
        /// </summary>
        public List<CBanInfo> Filtered { get; set; }

        public int Count { get { return this.Filtered.Count(); } }

        public int Skip
        {
            get { return _skip; }
            set
            {
                if (_skip != value)
                {
                    _skip = value;
                    this.OnChange();
                }
            }
        }

        public int Take
        {
            get { return _take; }
            set
            {
                if (_take != value)
                {
                    _take = value;
                    this.OnChange();
                }
            }
        }

        public String Filter
        {
            get { return this._filter; }
            set
            {
                if (_filter != value)
                {
                    _filter = value;
                    this.RefreshFilter();
                    this.OnChange();
                }
            }
        }

        public event Action Changed;

        public BansSource()
        {
            this.Items = new List<CBanInfo>();
            this.Filtered = new List<CBanInfo>();
        }

        /// <summary>
        /// Fires off the change event
        /// </summary>
        protected void OnChange()
        {
            var handler = this.Changed;
            if (handler != null)
            {
                handler();
            }
        }

        /// <summary>
        /// Updates the filtered content with items that exist in Items and the current filter
        /// </summary>
        protected void RefreshFilter()
        {
            if (String.IsNullOrEmpty(this.Filter) == false)
            {
                var filter = this.Filter.ToLower();

                this.Filtered = this.Items.Where(item =>
                    (item.SoldierName != null && item.SoldierName.ToLower().Contains(filter)) ||
                    (item.IpAddress != null && item.IpAddress.ToLower().Contains(filter)) ||
                    (item.Guid != null && item.Guid.ToLower().Contains(filter)) ||
                    (item.Reason != null && item.Reason.ToLower().Contains(filter))
                ).ToList();
            }
            else
            {
                this.Filtered = this.Items;
            }
        }

        public void Set<T>(IEnumerable<T> items)
        {
            if (typeof(T) != typeof(CBanInfo)) throw new InvalidCastException();

            // If we have nothing yet or the items we do have are expired.
            if (this.Items.Count == 0 || this.ItemsAge < DateTime.Now.AddMinutes(-1))
            {
                // We never get the pbguid's in one hit to know what is and isn't there.
                var pbItems = this.Items.Where(item => item.IdType == "pbguid").ToList();

                this.Items = items.Cast<CBanInfo>().Union(pbItems).ToList();

                //this.Items = items.Cast<CBanInfo>().ToList();
                this.ItemsAge = DateTime.Now;

                this.RefreshFilter();
                this.OnChange();
            }
        }

        /// <summary>
        /// Remove a single item without firing any list change events
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item">The item to append</param>
        /// <returns>True if the item has been appended, false otherwise.</returns>
        protected bool AppendItem<T>(T item)
        {
            bool appended = false;

            if (typeof(T) != typeof(CBanInfo)) throw new InvalidCastException();
            var cast = item as CBanInfo;

            if (cast != null)
            {
                if (this.Items.Any(ban => ban.SoldierName == cast.SoldierName && ban.IpAddress == cast.IpAddress && ban.Guid == cast.Guid) == false)
                {
                    this.Items.Add(item as CBanInfo);

                    appended = true;
                }
            }

            return appended;
        }

        public void Append<T>(T item)
        {
            if (this.AppendItem(item) == true)
            {
                this.RefreshFilter();
                this.OnChange();
            }
        }

        /// <summary>
        /// Removes any instances of an item found in the source
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns>True if the item has been removed</returns>
        protected bool RemoveItem<T>(T item)
        {
            bool removed = false;
            if (typeof(T) != typeof(CBanInfo)) throw new InvalidCastException();

            var cast = item as CBanInfo;

            if (cast != null)
            {
                if (cast.IdType == "pbguid")
                {
                    removed = this.Items.RemoveAll(ban => ban.Guid == cast.Guid) > 0;
                }
                else
                {
                    removed = this.Items.RemoveAll(ban => ban.SoldierName == cast.SoldierName && ban.IpAddress == cast.IpAddress && ban.Guid == cast.Guid) > 0;
                }
            }

            return removed;
        }

        public void Remove<T>(T item)
        {
            if (this.RemoveItem(item) == true)
            {
                this.RefreshFilter();
                this.OnChange();
            }
        }

        IEnumerable<T> ISource.Fetch<T>()
        {
            if (typeof(T) != typeof(CBanInfo)) throw new InvalidCastException();

            return this.Filtered.Skip(this.Skip).Take(this.Take).Cast<T>();
        }
    }
}
