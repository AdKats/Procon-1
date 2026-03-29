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
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace PRoCon.Core.Plugin
{
    // Factory class to create objects exposing IPRoConPluginInterface
    public class CPRoConPluginLoaderFactory
    {
        protected List<IPRoConPluginInterface> LoadedPlugins;

        private PluginLoadContext _loadContext;

        public CPRoConPluginLoaderFactory()
        {
            this.LoadedPlugins = new List<IPRoConPluginInterface>();
        }

        /// <summary>
        /// Sets the AssemblyLoadContext used to load plugin assemblies.
        /// </summary>
        public void SetLoadContext(PluginLoadContext loadContext)
        {
            _loadContext = loadContext;
        }

        /// <summary>
        /// Gets the current AssemblyLoadContext, if any.
        /// </summary>
        public PluginLoadContext GetLoadContext()
        {
            return _loadContext;
        }

        public IPRoConPluginInterface Create(string assemblyFile, string typeName, object[] constructArguments)
        {
            Assembly assembly;

            if (_loadContext != null)
            {
                assembly = _loadContext.LoadFromAssemblyPath(System.IO.Path.GetFullPath(assemblyFile));
            }
            else
            {
                assembly = Assembly.LoadFrom(assemblyFile);
            }

            Type pluginType = assembly.GetType(typeName);
            if (pluginType == null)
            {
                throw new TypeLoadException($"Could not find type '{typeName}' in assembly '{assemblyFile}'.");
            }

            IPRoConPluginInterface loadedPlugin = (IPRoConPluginInterface)Activator.CreateInstance(pluginType, constructArguments);

            this.LoadedPlugins.Add(loadedPlugin);

            return loadedPlugin;
        }

        public Object ConditionallyInvokeOn(List<String> types, String methodName, params object[] parameters)
        {
            Object returnValue = null;

            foreach (IPRoConPluginInterface plugin in this.LoadedPlugins.Where(plugin => types.Contains(plugin.ClassName) == true))
            {
                returnValue = plugin.Invoke(methodName, parameters);
            }

            return returnValue;
        }
    }
}
