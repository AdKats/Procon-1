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

    // TO DO: Why?  What the hell.
    [Serializable]
    public class CPrivileges
    {

        public CPrivileges()
        {
            // Set this class to have the lowest privileges flags.
            this.PrivilegesFlags = (UInt32)(Privileges.CannotIssueProconCommands |
                                             Privileges.CannotIssuePunkbusterCommands |
                                             Privileges.CannotPunishPlayers);
        }

        public CPrivileges(UInt32 ui32Privileges)
        {
            this.PrivilegesFlags = ui32Privileges;
        }

        public uint PrivilegesFlags { get; set; }

        public static UInt32 FullPrivilegesFlags
        {
            get
            {
                return (UInt32)(Privileges.CanLogin |
                                 Privileges.CanAlterServerSettings |
                                 Privileges.CanUseMapFunctions |
                                 Privileges.CanPermanentlyBanPlayers |
                                 Privileges.CanMovePlayers |
                                 Privileges.CanIssueAllPunkbusterCommands |
                                 Privileges.CanEditMapList |
                                 Privileges.CanEditBanList |
                                 Privileges.CanEditReservedSlotsList |
                                 Privileges.CanIssueAllProconCommands |
                                 Privileges.CanEditMapZones |
                                 Privileges.CanEditTextChatModerationList |
                                 Privileges.CanShutdownServer);
            }
        }

        public bool CanLogin
        {
            get { return ((this.PrivilegesFlags & 0x1) == 0x1); }
        }

        public bool CanAlterServerSettings
        {
            get { return ((this.PrivilegesFlags & 0x2) == 0x2); }
        }

        public bool CanUseMapFunctions
        {
            get { return ((this.PrivilegesFlags & 0x4) == 0x4); }
        }

        public bool CannotPunishPlayers
        {
            get
            {
                return ((this.PrivilegesFlags & 0x8) == 0x8 ||
                        (this.CanKillPlayers == false && this.CanKickPlayers == false &&
                         this.CanPermanentlyBanPlayers == false && this.CanTemporaryBanPlayers == false));
            }
        }

        public bool CanKillPlayers
        {
            get
            {
                return ((this.PrivilegesFlags & 0x10000) == 0x10000 || this.CanKickPlayers ||
                        this.CanPermanentlyBanPlayers || this.CanTemporaryBanPlayers);
            }
        }

        public bool CanKickPlayers
        {
            get
            {
                return ((this.PrivilegesFlags & 0x10) == 0x10 || this.CanPermanentlyBanPlayers ||
                        this.CanTemporaryBanPlayers);
            }
        }

        public bool CanTemporaryBanPlayers
        {
            get { return ((this.PrivilegesFlags & 0x20) == 0x20 || this.CanPermanentlyBanPlayers); }
        }

        public bool CanPermanentlyBanPlayers
        {
            get { return ((this.PrivilegesFlags & 0x40) == 0x40); }
        }

        public bool CanMovePlayers
        {
            get { return ((this.PrivilegesFlags & 0x40000) == 0x40000); }
        }

        public bool CannotIssuePunkbusterCommands
        {
            get
            {
                return ((this.PrivilegesFlags & 0x80) == 0x80 ||
                        (this.CanIssueLimitedPunkbusterCommands == false && this.CanIssueAllPunkbusterCommands == false));
            }
        }

        public bool CanIssueLimitedPunkbusterCommands
        {
            get { return ((this.PrivilegesFlags & 0x100) == 0x100 || this.CanIssueAllPunkbusterCommands); }
        }

        public bool CanIssueAllPunkbusterCommands
        {
            get { return ((this.PrivilegesFlags & 0x200) == 0x200); }
        }

        public bool CanEditMapList
        {
            get { return ((this.PrivilegesFlags & 0x400) == 0x400); }
        }

        public bool CanEditBanList
        {
            get { return ((this.PrivilegesFlags & 0x800) == 0x800); }
        }

        public bool CanEditReservedSlotsList
        {
            get { return ((this.PrivilegesFlags & 0x1000) == 0x1000); }
        }

        public bool CannotIssueProconCommands
        {
            get { return ((this.PrivilegesFlags & 0x2000) == 0x2000); }
        }

        public bool CanIssueLimitedProconPluginCommands
        {
            get
            {
                return ((this.PrivilegesFlags & 0x20000) == 0x20000 || this.CanIssueLimitedProconCommands ||
                        this.CanIssueAllProconCommands);
            }
        }

        public bool CanIssueLimitedProconCommands
        {
            get { return ((this.PrivilegesFlags & 0x4000) == 0x4000 || this.CanIssueAllProconCommands); }
        }

        public bool CanIssueAllProconCommands
        {
            get { return ((this.PrivilegesFlags & 0x8000) == 0x8000); }
        }

        public bool CanEditMapZones
        {
            get { return ((this.PrivilegesFlags & 0x80000) == 0x80000); }
        }

        public bool CanEditTextChatModerationList
        {
            get { return ((this.PrivilegesFlags & 0x100000) == 0x100000); }
        }

        public bool CanShutdownServer
        {
            get { return ((this.PrivilegesFlags & 0x200000) == 0x200000); }
        }

        public bool HasNoRconAccess
        {
            get { return ((~this.PrivilegesFlags & 0x1) == 0x1); }
        }

        public bool HasLimitedRconAccess
        {
            get { return (this.HasNoRconAccess == false && this.HasFullRconAccess == false); }
        }

        public bool HasFullRconAccess
        {
            get
            {
                return (this.CanLogin && this.CanAlterServerSettings && this.CanUseMapFunctions &&
                        this.CanPermanentlyBanPlayers && this.CanMovePlayers && this.CanIssueAllPunkbusterCommands &&
                        this.CanEditMapList && this.CanEditBanList && this.CanEditReservedSlotsList &&
                        this.CanIssueAllProconCommands && this.CanEditMapZones && this.CanEditTextChatModerationList && this.CanShutdownServer);
            }
        }

        public bool HasNoLocalAccess
        {
            get
            {
                return (!this.CanAlterServerSettings && !this.CanUseMapFunctions && this.CannotPunishPlayers &&
                        this.CannotIssuePunkbusterCommands && !this.CanMovePlayers && !this.CanEditMapList &&
                        !this.CanEditBanList && !this.CanEditReservedSlotsList && !this.CanEditTextChatModerationList && !this.CanShutdownServer);
            }
        }

        public bool HasLimitedLocalAccess
        {
            get { return (this.HasNoLocalAccess == false && this.HasFullLocalAccess == false); }
        }

        public bool HasFullLocalAccess
        {
            get
            {
                return (this.CanAlterServerSettings && this.CanUseMapFunctions && this.CanPermanentlyBanPlayers &&
                        this.CanMovePlayers && this.CanIssueAllPunkbusterCommands && this.CanEditMapList &&
                        this.CanEditBanList && this.CanEditReservedSlotsList && this.CanEditTextChatModerationList && this.CanShutdownServer);
            }
        }

        public bool Has(Privileges flags)
        {

            bool hasPrivileges = false;

            CPrivileges cpriv = new CPrivileges((UInt32)flags);
            cpriv.SetLowestPrivileges(this);

            if ((cpriv.PrivilegesFlags & (UInt32)flags) == (UInt32)flags)
            {
                hasPrivileges = true;
            }

            return hasPrivileges;
        }

        public void SetLowestPrivileges(CPrivileges spCompare)
        {
            Privileges newPrivilegesFlags = 0;

            newPrivilegesFlags |= (this.CanLogin && spCompare.CanLogin) ? Privileges.CanLogin : 0;

            newPrivilegesFlags |= (this.CanAlterServerSettings && spCompare.CanAlterServerSettings)
                                      ? Privileges.CanAlterServerSettings
                                      : 0;
            newPrivilegesFlags |= (this.CanUseMapFunctions && spCompare.CanUseMapFunctions)
                                      ? Privileges.CanUseMapFunctions
                                      : 0;

            newPrivilegesFlags |= (this.CannotPunishPlayers || spCompare.CannotPunishPlayers)
                                      ? Privileges.CannotPunishPlayers
                                      : 0;
            newPrivilegesFlags |= (this.CanKillPlayers && spCompare.CanKillPlayers) ? Privileges.CanKillPlayers : 0;
            newPrivilegesFlags |= (this.CanKickPlayers && spCompare.CanKickPlayers) ? Privileges.CanKickPlayers : 0;
            newPrivilegesFlags |= (this.CanTemporaryBanPlayers && spCompare.CanTemporaryBanPlayers)
                                      ? Privileges.CanTemporaryBanPlayers
                                      : 0;
            newPrivilegesFlags |= (this.CanPermanentlyBanPlayers && spCompare.CanPermanentlyBanPlayers)
                                      ? Privileges.CanPermanentlyBanPlayers
                                      : 0;
            newPrivilegesFlags |= (this.CanMovePlayers && spCompare.CanMovePlayers) ? Privileges.CanMovePlayers : 0;

            newPrivilegesFlags |= (this.CannotIssuePunkbusterCommands || spCompare.CannotIssuePunkbusterCommands)
                                      ? Privileges.CannotIssuePunkbusterCommands
                                      : 0;
            newPrivilegesFlags |= (this.CanIssueLimitedPunkbusterCommands && spCompare.CanIssueLimitedPunkbusterCommands)
                                      ? Privileges.CanIssueLimitedPunkbusterCommands
                                      : 0;
            newPrivilegesFlags |= (this.CanIssueAllPunkbusterCommands && spCompare.CanIssueAllPunkbusterCommands)
                                      ? Privileges.CanIssueAllPunkbusterCommands
                                      : 0;

            newPrivilegesFlags |= (this.CanEditBanList && spCompare.CanEditBanList) ? Privileges.CanEditBanList : 0;
            newPrivilegesFlags |= (this.CanEditMapList && spCompare.CanEditMapList) ? Privileges.CanEditMapList : 0;
            newPrivilegesFlags |= (this.CanEditReservedSlotsList && spCompare.CanEditReservedSlotsList)
                                      ? Privileges.CanEditReservedSlotsList
                                      : 0;

            newPrivilegesFlags |= (this.CannotIssueProconCommands || spCompare.CannotIssueProconCommands)
                                      ? Privileges.CannotIssueProconCommands
                                      : 0;
            newPrivilegesFlags |= (this.CanIssueLimitedProconPluginCommands &&
                                   spCompare.CanIssueLimitedProconPluginCommands)
                                      ? Privileges.CanIssueLimitedProconPluginCommands
                                      : 0;
            newPrivilegesFlags |= (this.CanIssueLimitedProconCommands && spCompare.CanIssueLimitedProconCommands)
                                      ? Privileges.CanIssueLimitedProconCommands
                                      : 0;
            newPrivilegesFlags |= (this.CanIssueAllProconCommands && spCompare.CanIssueAllProconCommands)
                                      ? Privileges.CanIssueAllProconCommands
                                      : 0;

            newPrivilegesFlags |= (this.CanEditMapZones && spCompare.CanEditMapZones)
                                      ? Privileges.CanEditMapZones
                                      : 0;

            newPrivilegesFlags |= (this.CanEditTextChatModerationList && spCompare.CanEditTextChatModerationList)
                                      ? Privileges.CanEditTextChatModerationList
                                      : 0;

            newPrivilegesFlags |= (this.CanShutdownServer && spCompare.CanShutdownServer)
                                      ? Privileges.CanShutdownServer
                                      : 0;

            this.PrivilegesFlags = (UInt32)newPrivilegesFlags;
        }
    }
}