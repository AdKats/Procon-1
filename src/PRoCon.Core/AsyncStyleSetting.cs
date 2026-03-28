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
    // TO DO: I hate this class so much, rewrite it some time.
    // NOTE: WinForms types (Control, PictureBox) replaced with object for .NET 8 cross-platform compatibility.
    // The UI layer is responsible for casting these back to the appropriate UI control types.
    public class AsyncStyleSetting
    {
        // Animation

        public static int INT_ANIMATEDSETTING_SHOWRESULT_TICKS = 10;
        public static int INT_ANIMATEDSETTING_TIMEOUT_TICKS = 40; // IF after 10 seconds we have heard nothing, timeout.
        public static int INT_ICON_ANIMATEDSETTING_SET_FAILURE = 2;
        public static int INT_ICON_ANIMATEDSETTING_SET_SUCCESS = 1;
        public static int INT_ICON_ANIMATEDSETTING_START = 0;
        public bool m_blIgnoreEvent;
        public bool m_blReEnableControls;
        public bool m_blSuccess; // Set to false or unknown
        public object m_ctrlResponseTarget;
        public int m_iTimeout;
        public object m_objOriginalValue;
        public object m_picStatus;
        public object[] ma_ctrlEnabledInputs;

        public AsyncStyleSetting(object picStatus, object ctrlResponseTarget, object[] a_ctrlEnabledInputs,
                                  bool blReEnableControls)
        {
            this.m_picStatus = picStatus;
            this.m_iTimeout = -1;
            this.m_ctrlResponseTarget = ctrlResponseTarget;
            this.ma_ctrlEnabledInputs = a_ctrlEnabledInputs;
            this.m_blReEnableControls = blReEnableControls;
            // Set to true so the first time does not trigger another.
            //this.m_blIgnoreEvent = true;
        }

        public bool IgnoreEvent
        {
            get
            {
                bool blOriginal = this.m_blIgnoreEvent;

                this.m_blIgnoreEvent = false;

                return blOriginal;
            }
            set { this.m_blIgnoreEvent = value; }
        }
    }
}
