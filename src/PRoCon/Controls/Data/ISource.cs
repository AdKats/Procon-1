using System;
using System.Collections.Generic;

namespace PRoCon.Controls.Data
{
    public interface ISource
    {
        /// <summary>
        /// The total number of items at the source.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// The number of items to skip in this source
        /// </summary>
        int Skip { get; set; }

        /// <summary>
        /// The number of items to take from this source.
        /// </summary>
        int Take { get; set; }

        /// <summary>
        /// Filter the source by a parameter
        /// </summary>
        String Filter { get; set; }

        /// <summary>
        /// Called whenever the source has been modified and new items may be available.
        /// </summary>
        event Action Changed;

        /// <summary>
        /// Set the current list of items
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        void Set<T>(IEnumerable<T> items);

        /// <summary>
        /// Append a single item to the source list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item">The item to append</param>
        void Append<T>(T item);

        /// <summary>
        /// Remove a single item from the source list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item">The item to remove</param>
        void Remove<T>(T item);

        /// <summary>
        /// Fetch an enumerable of items from an offset
        /// </summary>
        /// <typeparam name="T">The type of item we are expecting</typeparam>
        /// <returns>Enumerable of type T</returns>
        IEnumerable<T> Fetch<T>();
    }
}
