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
using System.Collections.Generic;

namespace PRoCon.Core.Plugin
{
    public interface IPRoConPluginInterface
    {

        String ClassName { get; set; }

        object Invoke(string strMethodName, params object[] a_objParameters);

        // Compile and init events
        void OnPluginLoaded(string strHostName, string strPort, string strPRoConVersion);
        void OnPluginEnable();
        void OnPluginDisable();

        // Plugin details
        string GetPluginName();
        string GetPluginVersion();
        string GetPluginAuthor();
        string GetPluginWebsite();
        string GetPluginDescription();

        // Plugin variables
        List<CPluginVariable> GetDisplayPluginVariables();
        List<CPluginVariable> GetPluginVariables();
        void SetPluginVariable(string strVariable, string strValue);
    }
}
