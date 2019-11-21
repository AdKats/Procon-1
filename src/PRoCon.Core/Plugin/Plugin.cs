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
    public class Plugin
    {

        public string ClassName { get; private set; }

        public bool IsLoaded { get; set; }
        public bool IsEnabled { get; set; }
        public IPRoConPluginInterface Type { get; private set; }

        public List<string> RegisteredEvents { get; set; }

        public Dictionary<string, string> CacheFailCompiledPluginVariables { get; private set; }

        // Failed plugin
        public Plugin(string className)
        {
            this.ClassName = className;
            this.CacheFailCompiledPluginVariables = new Dictionary<string, string>();
            this.IsLoaded = false;
            this.IsEnabled = false;
        }

        // Loaded plugin
        public Plugin(string className, IPRoConPluginInterface type)
        {
            this.ClassName = className;
            this.Type = type;
            this.Type.ClassName = this.ClassName;
            this.IsLoaded = true;
            this.IsEnabled = false;

            this.RegisteredEvents = new List<string>();
        }

        public void ConditionalInvoke(string methodName, params object[] parameters)
        {
            // if no events have been registered (all events fired) or the event has been registered.
            if (this.RegisteredEvents != null && (this.RegisteredEvents.Count == 0 || this.RegisteredEvents.Contains(methodName) == true))
            {
                this.Type.Invoke(methodName, parameters);
            }
        }

        public bool CanConditionallyInvoke(String methodName)
        {
            return this.RegisteredEvents != null && (this.RegisteredEvents.Count == 0 || this.RegisteredEvents.Contains(methodName) == true);
        }
    }
}
