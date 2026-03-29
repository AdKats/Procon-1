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
    using System.Collections.Generic;

    public class ScheduledTask
    {
        private int m_iDelay;
        private int m_iInterval;
        private int m_iRepeat;

        private int m_iTickCountdown;
        private List<string> m_lstCommandWords;
        private string m_strTaskName;

        public ScheduledTask(string strTaskName, List<string> lstCommandWords, int iDelay, int iInterval, int iRepeat)
        {
            if (iDelay < 0 || iInterval <= 0) return;

            this.m_strTaskName = strTaskName;
            this.m_lstCommandWords = lstCommandWords;
            this.m_iDelay = iDelay;
            this.m_iInterval = iInterval;
            this.m_iRepeat = iRepeat;

            this.m_iTickCountdown = this.m_iDelay;
        }

        public string TaskName
        {
            get { return this.m_strTaskName; }
        }

        public bool RemoveTask
        {
            get { return this.m_iRepeat == 0; }
        }

        public bool ExecuteCommand
        {
            get
            {
                bool blExecuteCommand = false;

                if (this.m_iInterval > 0 && this.m_iRepeat != 0)
                {
                    if (--this.m_iTickCountdown <= 0)
                    {
                        blExecuteCommand = true;
                        this.m_iTickCountdown = this.m_iInterval;

                        // If the command is not on a loop forever.
                        if (this.m_iRepeat != -1)
                        {
                            // Drop the repeat.  When repeat == 0 the task won't be repeated.
                            this.m_iRepeat--;
                        }
                    }
                }

                return blExecuteCommand;
            }
        }

        public List<string> Command
        {
            get { return new List<string>(this.m_lstCommandWords); }
        }

        public override string ToString()
        {
            return String.Format("N:[{0}] D:{1} I:{2} R:{3} Command: {4}", this.m_strTaskName, this.m_iDelay,
                                 this.m_iInterval, this.m_iRepeat, String.Join(" ", this.m_lstCommandWords.ToArray()));
        }
    }
}
