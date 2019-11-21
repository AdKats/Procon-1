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

namespace PRoCon.Core.TextChatModeration
{
    [Serializable]
    public class TextChatModerationEntry
    {

        public PlayerModerationLevelType PlayerModerationLevel
        {
            get;
            private set;
        }

        public string SoldierName
        {
            get;
            private set;
        }

        public TextChatModerationEntry(PlayerModerationLevelType playerModerationLevel, string soldierName)
        {
            this.PlayerModerationLevel = playerModerationLevel;
            this.SoldierName = soldierName;
        }

        public TextChatModerationEntry(string playerModerationLevel, string soldierName)
        {

            this.SoldierName = soldierName;
            this.PlayerModerationLevel = TextChatModerationEntry.GetPlayerModerationLevelType(playerModerationLevel);
        }

        public static PlayerModerationLevelType GetPlayerModerationLevelType(string playerModerationLevel)
        {

            PlayerModerationLevelType returnPlayerModerationLevel = PlayerModerationLevelType.None;

            switch (playerModerationLevel.ToLower())
            {
                case "muted":
                    returnPlayerModerationLevel = PlayerModerationLevelType.Muted;
                    break;
                case "normal":
                    returnPlayerModerationLevel = PlayerModerationLevelType.Normal;
                    break;
                case "voice":
                    returnPlayerModerationLevel = PlayerModerationLevelType.Voice;
                    break;
                case "admin":
                    returnPlayerModerationLevel = PlayerModerationLevelType.Admin;
                    break;
                default:
                    break;
            }

            return returnPlayerModerationLevel;
        }

        public static ServerModerationModeType GetServerModerationLevelType(string serverModerationLevel)
        {

            ServerModerationModeType returnServerModerationLevel = ServerModerationModeType.None;

            switch (serverModerationLevel.ToLower())
            {
                case "muted":
                    returnServerModerationLevel = ServerModerationModeType.Muted;
                    break;
                case "moderated":
                    returnServerModerationLevel = ServerModerationModeType.Moderated;
                    break;
                case "free":
                    returnServerModerationLevel = ServerModerationModeType.Free;
                    break;
                default:
                    break;
            }

            return returnServerModerationLevel;
        }
    }
}
