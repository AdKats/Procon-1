using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace PRoCon.Core.Plugin
{
    public class PluginLoadContext : AssemblyLoadContext
    {
        private readonly string _pluginDirectory;

        public PluginLoadContext(string pluginDirectory) : base(isCollectible: true)
        {
            _pluginDirectory = pluginDirectory;
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            // Try to find the assembly in the plugin directory first
            string assemblyPath = Path.Combine(_pluginDirectory, $"{assemblyName.Name}.dll");
            if (File.Exists(assemblyPath))
            {
                return LoadFromAssemblyPath(assemblyPath);
            }

            // Fall back to the default context (shared assemblies like PRoCon.Core, Newtonsoft.Json, etc.)
            return null;
        }
    }
}
