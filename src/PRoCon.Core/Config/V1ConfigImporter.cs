using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using PRoCon.Core.Options;
using PRoCon.Core.Remote;

namespace PRoCon.Core.Config
{
    /// <summary>
    /// Imports a PRoCon v1.x installation into v2.0.
    ///
    /// Usage: drop a v1 PRoCon folder (or just its Configs/ and Plugins/ dirs)
    /// into the data directory's Import/ folder. On next startup, PRoCon will:
    ///   1. Read procon.cfg + accounts.cfg → merge into procon.json (encrypted)
    ///   2. Copy per-server config directories (host_port/) into Configs/
    ///   3. Copy plugins into Plugins/<GameType>/
    ///   4. Rename Import/ → Import.done/ to prevent re-import
    ///
    /// Expected Import/ layout (any of these):
    ///   Import/Configs/procon.cfg
    ///   Import/Configs/accounts.cfg
    ///   Import/Configs/1.2.3.4_47200/  (per-server configs)
    ///   Import/Plugins/BF4/*.cs
    ///   Import/procon.cfg              (flat — if Configs/ subdir doesn't exist)
    ///   Import/accounts.cfg
    /// </summary>
    public static class V1ConfigImporter
    {
        public static bool HasImportData()
        {
            string importDir = Path.Combine(ProConPaths.DataDirectory, "Import");
            return Directory.Exists(importDir);
        }

        /// <summary>
        /// Runs the import and returns a summary of what was imported.
        /// </summary>
        public static ImportResult Import()
        {
            var result = new ImportResult();
            string importDir = Path.Combine(ProConPaths.DataDirectory, "Import");

            if (!Directory.Exists(importDir))
                return result;

            try
            {
                // Find configs — check Import/Configs/ first, then Import/ directly
                string cfgDir = Path.Combine(importDir, "Configs");
                if (!Directory.Exists(cfgDir))
                    cfgDir = importDir;

                // 1. Import procon.cfg (servers + options) → copy to Configs/
                string proconCfg = Path.Combine(cfgDir, "procon.cfg");
                if (File.Exists(proconCfg))
                {
                    string dest = Path.Combine(ProConPaths.ConfigsDirectory, "procon.cfg");
                    if (!File.Exists(dest))
                        File.Copy(proconCfg, dest);
                    result.ServersImported = CountServersInCfg(proconCfg);
                    result.HasMainConfig = true;
                }

                // 2. Import accounts.cfg → copy to Configs/
                string accountsCfg = Path.Combine(cfgDir, "accounts.cfg");
                if (File.Exists(accountsCfg))
                {
                    string dest = Path.Combine(ProConPaths.ConfigsDirectory, "accounts.cfg");
                    if (!File.Exists(dest))
                        File.Copy(accountsCfg, dest);
                    result.AccountsImported = CountAccountsInCfg(accountsCfg);
                    result.HasAccountsConfig = true;
                }

                // 3. Copy per-server config directories
                foreach (var dir in Directory.GetDirectories(cfgDir))
                {
                    string dirName = Path.GetFileName(dir);
                    // Server configs are named like "1.2.3.4_47200" (IP_Port)
                    if (Regex.IsMatch(dirName, @"^[\d\.]+_\d+$"))
                    {
                        string destDir = Path.Combine(ProConPaths.ConfigsDirectory, dirName);
                        CopyDirectory(dir, destDir);
                        result.ServerConfigsCopied++;
                    }
                }

                // Also copy flat server .cfg files (host_port.cfg)
                foreach (var file in Directory.GetFiles(cfgDir, "*.cfg"))
                {
                    string name = Path.GetFileNameWithoutExtension(file);
                    if (Regex.IsMatch(name, @"^[\d\.]+_\d+$"))
                    {
                        string dest = Path.Combine(ProConPaths.ConfigsDirectory, Path.GetFileName(file));
                        if (!File.Exists(dest))
                        {
                            File.Copy(file, dest);
                            result.ServerConfigsCopied++;
                        }
                    }
                }

                // 4. Plugin source files are NOT copied — v1 plugins are outdated.
                //    Download v2-compatible plugins from the official release page.

                // 5. Rename Import/ → Import.done/
                string doneDir = Path.Combine(ProConPaths.DataDirectory, "Import.done");
                if (Directory.Exists(doneDir))
                    Directory.Delete(doneDir, true);
                Directory.Move(importDir, doneDir);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Error = ex.Message;
            }

            return result;
        }

        private static int CountServersInCfg(string path)
        {
            int servers = 0;
            try
            {
                string[] lines = File.ReadAllLines(path, Encoding.UTF8);
                foreach (string line in lines)
                {
                    List<string> words = Packet.Wordify(line);
                    if (words.Count < 1) continue;
                    if (line.TrimStart().StartsWith("//")) continue;

                    // Count server entries
                    if (words.Count >= 3 && string.Equals(words[0], "procon.private.servers.add", StringComparison.OrdinalIgnoreCase))
                        servers++;
                }
            }
            catch { }
            return servers;
        }

        private static int CountAccountsInCfg(string path)
        {
            int accounts = 0;
            try
            {
                string[] lines = File.ReadAllLines(path, Encoding.UTF8);
                foreach (string line in lines)
                {
                    List<string> words = Packet.Wordify(line);
                    if (words.Count >= 3 && string.Equals(words[0], "procon.public.accounts.create", StringComparison.OrdinalIgnoreCase))
                        accounts++;
                }
            }
            catch { }
            return accounts;
        }

        private static void CopyDirectory(string sourceDir, string destDir)
        {
            Directory.CreateDirectory(destDir);
            foreach (var file in Directory.GetFiles(sourceDir))
            {
                string dest = Path.Combine(destDir, Path.GetFileName(file));
                if (!File.Exists(dest))
                    File.Copy(file, dest);
            }
            foreach (var dir in Directory.GetDirectories(sourceDir))
            {
                CopyDirectory(dir, Path.Combine(destDir, Path.GetFileName(dir)));
            }
        }
    }

    public class ImportResult
    {
        public bool Success { get; set; }
        public bool HasMainConfig { get; set; }
        public bool HasAccountsConfig { get; set; }
        public int ServersImported { get; set; }
        public int AccountsImported { get; set; }
        public int ServerConfigsCopied { get; set; }
        public string Error { get; set; }

        public override string ToString()
        {
            if (!Success && Error != null)
                return $"Import failed: {Error}";
            if (!HasMainConfig && !HasAccountsConfig)
                return "Nothing to import.";

            var parts = new List<string>();
            if (ServersImported > 0) parts.Add($"{ServersImported} server(s)");
            if (AccountsImported > 0) parts.Add($"{AccountsImported} account(s)");
            if (ServerConfigsCopied > 0) parts.Add($"{ServerConfigsCopied} server config(s)");
            return $"Imported: {string.Join(", ", parts)}";
        }
    }
}
