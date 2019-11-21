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

namespace PRoCon.Core.Plugin.Commands
{
    public class ConfirmationEntry
    {

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

        public MatchCommand MatchedCommand
        {
            get;
            private set;
        }

        public CapturedCommand ConfirmationDetails
        {
            get;
            private set;
        }

        public CPlayerSubset MessageScope
        {
            get;
            private set;
        }

        public ConfirmationEntry(string speaker, string message, MatchCommand mtcCommand, CapturedCommand capCommand, CPlayerSubset subset)
        {
            this.Speaker = speaker;
            this.Message = message;
            this.MatchedCommand = mtcCommand;
            this.ConfirmationDetails = capCommand;
            this.MessageScope = subset;
        }
    }
}
