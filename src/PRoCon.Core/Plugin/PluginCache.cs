using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace PRoCon.Core.Plugin
{
    [Serializable]
    public class PluginCache
    {

        /// <summary>
        /// The list of plugins being cached
        /// </summary>
        public List<PluginCacheEntry> Entries { get; set; }

        public PluginCache()
        {
            this.Entries = new List<PluginCacheEntry>();
        }

        public bool IsModified(String className, String hash)
        {
            bool isModified = true;

            PluginCacheEntry pluginEntry = this.Entries.FirstOrDefault(entry => entry.ClassName == className);

            if (pluginEntry != null)
            {
                isModified = String.Compare(pluginEntry.Hash, hash, StringComparison.OrdinalIgnoreCase) != 0;
            }

            return isModified;
        }

        public void Cache(PluginCacheEntry entry)
        {
            PluginCacheEntry pluginEntry = null;

            do
            {
                pluginEntry = this.Entries.FirstOrDefault(e => e.ClassName == entry.ClassName);

                if (pluginEntry != null)
                {
                    this.Entries.Remove(pluginEntry);
                }

            } while (pluginEntry != null);

            this.Entries.Add(entry);
        }

        /// <summary>
        /// This is a minor optimization from the Utils/XElement serialization. Since this is a simple
        /// file the overhead of the reflection outweighs the few lines of code saved.
        /// We still save with Utils/ToXElement because it's easy, but we load it manually for speed.
        /// </summary>
        /// <param name="path">The path to the plugin cache file.</param>
        public static PluginCache Load(String path)
        {
            PluginCache cache = new PluginCache();

            XElement root = XDocument.Load(path).Root;

            if (root != null)
            {
                foreach (var element in root.Descendants("PluginCacheEntry"))
                {
                    var className = element.Element("ClassName");
                    var destinationPath = element.Element("DestinationPath");
                    var hash = element.Element("Hash");
                    var sourcePath = element.Element("SourcePath");
                    var stamp = element.Element("Stamp");

                    if (className != null && destinationPath != null && hash != null && sourcePath != null && stamp != null)
                    {
                        cache.Entries.Add(new PluginCacheEntry()
                        {
                            ClassName = className.Value,
                            DestinationPath = destinationPath.Value,
                            Hash = hash.Value,
                            SourcePath = sourcePath.Value,
                            Stamp = DateTime.Parse(stamp.Value)
                        });
                    }
                }
            }

            return cache;
        }

        public void Save(String path)
        {
            XDocument pluginCacheDocument = new XDocument(
                new XElement("PluginCache",
                    new XElement("Entries",
                        this.Entries.Select(item => new XElement("PluginCacheEntry",
                            new XElement("ClassName", item.ClassName),
                            new XElement("DestinationPath", item.DestinationPath),
                            new XElement("Hash", item.Hash),
                            new XElement("SourcePath", item.SourcePath),
                            new XElement("Stamp", item.Stamp)
                        )).Cast<Object>().ToArray()
                    )
                )
            );

            pluginCacheDocument.Add();

            pluginCacheDocument.Save(path);
        }
    }
}
