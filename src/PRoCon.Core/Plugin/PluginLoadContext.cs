using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace PRoCon.Core.Plugin
{
    public class PluginLoadContext : AssemblyLoadContext
    {
        private readonly string _pluginDirectory;

        // Assemblies that must come from the default (host) context to avoid type identity issues.
        // If PRoCon.Core loads in both contexts, IPRoConPluginInterface becomes two different types.
        private static readonly HashSet<string> SharedAssemblies = new(StringComparer.OrdinalIgnoreCase)
        {
            "PRoCon.Core",
            "Newtonsoft.Json",
            "MySqlConnector",
            "Microsoft.CodeAnalysis",
            "Microsoft.CodeAnalysis.CSharp",
        };

        public PluginLoadContext(string pluginDirectory) : base(isCollectible: true)
        {
            _pluginDirectory = pluginDirectory;
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            // Never load shared/host assemblies from the plugin directory — they must come
            // from the default context so types like IPRoConPluginInterface have one identity.
            if (SharedAssemblies.Contains(assemblyName.Name))
            {
                return null;
            }

            // Try to find plugin-specific assemblies in the plugin directory
            string assemblyPath = Path.Combine(_pluginDirectory, $"{assemblyName.Name}.dll");
            if (File.Exists(assemblyPath))
            {
                return LoadFromAssemblyPath(assemblyPath);
            }

            // Fall back to the default context
            return null;
        }
    }
}
