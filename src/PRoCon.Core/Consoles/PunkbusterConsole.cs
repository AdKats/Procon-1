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

using PRoCon.Core.Logging;
using PRoCon.Core.Remote;
using System;

// This class will move to .Core once ProConClient is in .Core.

namespace PRoCon.Core.Consoles
{
    public class PunkbusterConsole : Loggable
    {
        protected readonly PRoConClient Client;

        public PunkbusterConsole(PRoConClient client) : base()
        {
            Client = client;

            FileHostNamePort = Client.FileHostNamePort;
            LoggingStartedPrefix = "Punkbuster logging started";
            LoggingStoppedPrefix = "Punkbuster logging stopped";
            FileNameSuffix = "punkbuster";

            Client.Game.PunkbusterMessage += new FrostbiteClient.PunkbusterMessageHandler(m_prcClient_PunkbusterMessage);
            Client.Game.SendPunkbusterMessage += new FrostbiteClient.SendPunkBusterMessageHandler(m_prcClient_SendPunkbusterMessage);
        }

        public event WriteConsoleHandler WriteConsole;

        private void m_prcClient_SendPunkbusterMessage(FrostbiteClient sender, string punkbusterMessage)
        {
            Write("^2" + punkbusterMessage.TrimEnd('\r', '\n').Replace("{", "{{").Replace("}", "}}"));
        }

        private void m_prcClient_PunkbusterMessage(FrostbiteClient sender, string punkbusterMessage)
        {
            Write(punkbusterMessage.TrimEnd('\r', '\n').Replace("{", "{{").Replace("}", "}}"));
        }

        public void Write(string strFormat, params string[] arguments)
        {
            DateTime dtLoggedTime = DateTime.UtcNow.ToUniversalTime().AddHours(Client.Game.UtcOffset).ToLocalTime();
            string strText = String.Format(strFormat, arguments);

            WriteLogLine(String.Format("[{0}] {1}", dtLoggedTime.ToString("HH:mm:ss"), strText));

            if (WriteConsole != null)
            {
                this.WriteConsole(dtLoggedTime, strText);
            }
        }
    }
}