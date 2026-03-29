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

namespace PRoCon.Core
{
    using System;

    [Flags]
    public enum Privileges
    {
        CanLogin = 0x01,
        CanAlterServerSettings = 0x02,
        CanUseMapFunctions = 0x04,
        CannotPunishPlayers = 0x08,
        CanKickPlayers = 0x10,
        CanTemporaryBanPlayers = 0x20,
        CanPermanentlyBanPlayers = 0x40,
        CannotIssuePunkbusterCommands = 0x80,
        CanIssueLimitedPunkbusterCommands = 0x100,
        CanIssueAllPunkbusterCommands = 0x200,
        CanEditMapList = 0x400,
        CanEditBanList = 0x800,
        CanEditReservedSlotsList = 0x1000,
        CannotIssueProconCommands = 0x2000,
        CanIssueLimitedProconCommands = 0x4000,
        CanIssueAllProconCommands = 0x8000,
        CanKillPlayers = 0x10000,
        CanIssueLimitedProconPluginCommands = 0x20000,
        CanMovePlayers = 0x40000,
        CanEditMapZones = 0x80000,
        CanEditTextChatModerationList = 0x100000,
        CanShutdownServer = 0x200000,
    }
}
