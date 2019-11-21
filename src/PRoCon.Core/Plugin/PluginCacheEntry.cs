using System;

namespace PRoCon.Core.Plugin
{
    [Serializable]
    public class PluginCacheEntry
    {

        /// <summary>
        /// The class name of the plugin
        /// </summary>
        public String ClassName { get; set; }

        /// <summary>
        /// The path to the file being cached.
        /// </summary>
        public String SourcePath { get; set; }

        /// <summary>
        /// The target path of the compiled file.
        /// </summary>
        public String DestinationPath { get; set; }

        /// <summary>
        /// The md5 hash of the source file
        /// </summary>
        public String Hash { get; set; }

        /// <summary>
        /// When the plugin was compiled.
        /// </summary>
        public DateTime Stamp { get; set; }

        public PluginCacheEntry()
        {
            this.Stamp = DateTime.Now;
        }
    }
}
