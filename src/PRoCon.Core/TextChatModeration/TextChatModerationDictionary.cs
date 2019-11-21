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

namespace PRoCon.Core.TextChatModeration
{
    // TextChatModerationEntry
    [Serializable]
    public class TextChatModerationDictionary : KeyedCollection<string, TextChatModerationEntry>
    {

        protected override string GetKeyForItem(TextChatModerationEntry item)
        {
            return item.SoldierName;
        }

        public void AddEntry(TextChatModerationEntry item)
        {
            if (this.Contains(item.SoldierName) == true)
            {
                this.SetItem(this.IndexOf(this[item.SoldierName]), item);
            }
            else
            {
                this.Add(item);
            }
        }

        public void RemoveEntry(TextChatModerationEntry item)
        {
            if (this.Contains(item.SoldierName) == true)
            {
                this.Remove(item.SoldierName);
            }
        }

        public void AddRange(IEnumerable<TextChatModerationEntry> list)
        {
            foreach (TextChatModerationEntry item in list)
            {
                this.AddEntry(item);
            }
        }

    }
}
