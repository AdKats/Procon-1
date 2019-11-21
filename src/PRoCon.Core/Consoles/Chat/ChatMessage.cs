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

namespace PRoCon.Core.Consoles.Chat
{
    public class ChatMessage
    {

        public DateTime LoggedTime
        {
            get;
            private set;
        }

        public string Speaker
        {
            get;
            private set;
        }

        public string Message
        {
            get;
            private set;
        }

        public bool IsFromServer
        {
            get;
            private set;
        }

        public bool IsYelling
        {
            get;
            private set;
        }

        public CPlayerSubset Subset
        {
            get;
            private set;
        }

        public ChatMessage(DateTime loggedTime, string speaker, string message, bool isFromServer, bool isYelling, CPlayerSubset subset)
        {
            this.LoggedTime = loggedTime;
            this.Speaker = speaker;
            this.Message = message;
            this.IsFromServer = isFromServer;
            this.IsYelling = isYelling;
            this.Subset = subset;
        }

        public Hashtable ToHashtable()
        {

            Hashtable message = new Hashtable();

            message.Add("date_time", JSON.DateTimeToISO8601(this.LoggedTime.ToUniversalTime()));
            message.Add("speaker", this.Speaker);
            message.Add("message", this.Message);
            message.Add("is_from_server", this.IsFromServer);
            message.Add("is_yelling", this.IsYelling);
            message.Add("subset", this.Subset.ToHashtable());

            return message;
        }
    }
}
