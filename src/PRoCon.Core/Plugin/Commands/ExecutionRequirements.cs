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

namespace PRoCon.Core.Plugin.Commands
{
    [Serializable]
    public class ExecutionRequirements
    {

        public ExecutionScope ExecutionScope
        {
            get;
            private set;
        }

        public Privileges RequiredPrivileges
        {
            get;
            private set;
        }

        public string FailedRequirementsMessage
        {
            get;
            private set;
        }

        public int MinimumMatchSimilarity
        {
            get;
            private set;
        }

        public MatchCommand ConfirmationCommand
        {
            get;
            private set;
        }

        public ExecutionRequirements(ExecutionScope scope)
        {
            this.Initialize(scope, Privileges.CanLogin, int.MaxValue, null, string.Empty);
        }

        public ExecutionRequirements(ExecutionScope scope, string strFailedRequirementsMessage)
        {
            this.Initialize(scope, Privileges.CanLogin, int.MaxValue, null, strFailedRequirementsMessage);
        }

        public ExecutionRequirements(ExecutionScope scope, Privileges privileges, string strFailedRequirementsMessage)
        {
            this.Initialize(scope, privileges, int.MaxValue, null, strFailedRequirementsMessage);
        }

        public ExecutionRequirements(ExecutionScope scope, int iMinimumSimilarity, MatchCommand mcConfirmationCommand)
        {
            this.Initialize(scope, Privileges.CanLogin, iMinimumSimilarity, mcConfirmationCommand, string.Empty);
        }

        public ExecutionRequirements(ExecutionScope scope, int iMinimumSimilarity, MatchCommand mcConfirmationCommand, string strFailedRequirementsMessage)
        {
            this.Initialize(scope, Privileges.CanLogin, iMinimumSimilarity, mcConfirmationCommand, strFailedRequirementsMessage);
        }

        public ExecutionRequirements(ExecutionScope scope, Privileges privileges, int iMinimumSimilarity, MatchCommand mcConfirmationCommand, string strFailedRequirementsMessage)
        {
            this.Initialize(scope, privileges, iMinimumSimilarity, mcConfirmationCommand, strFailedRequirementsMessage);
        }

        private void Initialize(ExecutionScope scope, Privileges privileges, int iMinimumSimilarity, MatchCommand mcConfirmationCommand, string strFailedRequirementsMessage)
        {
            this.ExecutionScope = scope;
            this.RequiredPrivileges = privileges;
            this.MinimumMatchSimilarity = iMinimumSimilarity;
            this.ConfirmationCommand = mcConfirmationCommand;

            if (strFailedRequirementsMessage.Length < 100)
            {
                this.FailedRequirementsMessage = strFailedRequirementsMessage;
            }
            else
            {
                this.FailedRequirementsMessage = strFailedRequirementsMessage.Substring(0, 100);
            }
        }

        public bool HasValidPermissions(CPrivileges privileges)
        {

            bool canExecuteCommand = false;

            if (this.ExecutionScope == ExecutionScope.All)
            {
                canExecuteCommand = true;
            }
            else if (this.ExecutionScope == ExecutionScope.Account)
            {
                if (privileges != null)
                {
                    canExecuteCommand = true;
                }
            }
            else if (this.ExecutionScope == ExecutionScope.Privileges)
            {
                if (privileges != null && privileges.Has(this.RequiredPrivileges) == true)
                {
                    canExecuteCommand = true;
                }
            }

            return canExecuteCommand;
        }
    }
}
