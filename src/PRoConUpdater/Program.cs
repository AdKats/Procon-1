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
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using ICSharpCode.SharpZipLib.Zip;

namespace PRoConUpdater
{
    static class Program
    {
        private static StringBuilder errorLog = new StringBuilder();

        static int Main(string[] args)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string updateDir = Path.Combine(baseDir, "Updates");

            if (!Directory.Exists(updateDir))
            {
                Console.WriteLine("No Updates directory found. Nothing to do.");
                return 0;
            }

            Log("PRoCon Updater starting...");

            CreateConfigBackup(baseDir, updateDir);
            WaitForProconToClose(baseDir);

            Log("Beginning file updates...");
            MoveContents(updateDir);

            try
            {
                if (Directory.Exists(updateDir))
                    Directory.Delete(updateDir, true);
            }
            catch (Exception ex)
            {
                Log($"Warning: Could not remove Updates directory: {ex.Message}");
            }

            bool restartProcon = ShouldRestartProcon();

            if (restartProcon)
            {
                string proconExe = Path.Combine(baseDir, "PRoCon.Console.exe");
                if (!File.Exists(proconExe))
                    proconExe = Path.Combine(baseDir, "PRoCon.UI.exe");
                if (!File.Exists(proconExe))
                    proconExe = Path.Combine(baseDir, "PRoCon.exe");

                if (File.Exists(proconExe))
                {
                    Log($"Restarting PRoCon: {proconExe}");
                    Process.Start(proconExe, args.Length > 0 ? string.Join(" ", args) : "");
                }
                else
                {
                    Log("Cannot find PRoCon executable to restart.");
                }
            }

            try
            {
                File.WriteAllText(Path.Combine(baseDir, "update.log"), errorLog.ToString());
            }
            catch { }

            Log("Update complete.");
            return 0;
        }

        static void CreateConfigBackup(string baseDir, string updateDir)
        {
            try
            {
                Log("Backing up configs...");

                string configsDir = Path.Combine(baseDir, "Configs");
                if (!Directory.Exists(configsDir))
                {
                    Log("No Configs directory found, skipping backup.");
                    return;
                }

                string backupsDir = Path.Combine(configsDir, "Backups");
                Directory.CreateDirectory(backupsDir);

                string currentVersion = "unknown";
                string updatedVersion = "unknown";

                string currentExe = Path.Combine(baseDir, "PRoCon.exe");
                string updatedExe = Path.Combine(updateDir, "PRoCon.exe");

                if (File.Exists(currentExe))
                    currentVersion = FileVersionInfo.GetVersionInfo(currentExe).FileVersion ?? "unknown";
                if (File.Exists(updatedExe))
                    updatedVersion = FileVersionInfo.GetVersionInfo(updatedExe).FileVersion ?? "unknown";

                string zipFileName = $"{currentVersion}_to_{updatedVersion}_backup.zip";

                using (var zipStream = new ZipOutputStream(File.Create(Path.Combine(backupsDir, zipFileName))))
                {
                    zipStream.SetLevel(5);

                    foreach (string file in Directory.GetFiles(configsDir, "*.cfg"))
                        AddFileToZip(zipStream, file, "");

                    foreach (string dir in Directory.GetDirectories(configsDir))
                    {
                        if (Path.GetFileName(dir) == "Backups") continue;
                        AddDirectoryToZip(zipStream, dir, Path.GetFileName(dir));
                    }
                }

                Log($"Configs backed up to {zipFileName}");
            }
            catch (Exception e)
            {
                Log($"Error backing up configs: {e.Message}");
            }
        }

        static void AddFileToZip(ZipOutputStream zipStream, string filePath, string entryPrefix)
        {
            string entryName = string.IsNullOrEmpty(entryPrefix)
                ? Path.GetFileName(filePath)
                : $"{entryPrefix}/{Path.GetFileName(filePath)}";

            zipStream.PutNextEntry(new ZipEntry(entryName) { DateTime = File.GetLastWriteTime(filePath) });

            byte[] buffer = new byte[4096];
            using (var fs = File.OpenRead(filePath))
            {
                int bytesRead;
                while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) > 0)
                    zipStream.Write(buffer, 0, bytesRead);
            }
            zipStream.CloseEntry();
        }

        static void AddDirectoryToZip(ZipOutputStream zipStream, string dirPath, string entryPrefix)
        {
            foreach (string file in Directory.GetFiles(dirPath))
                AddFileToZip(zipStream, file, entryPrefix);

            foreach (string subDir in Directory.GetDirectories(dirPath))
                AddDirectoryToZip(zipStream, subDir, $"{entryPrefix}/{Path.GetFileName(subDir)}");
        }

        static void WaitForProconToClose(string baseDir)
        {
            Log("Checking if PRoCon is running...");
            int waitCounter = 0;

            while (true)
            {
                bool running = false;

                try
                {
                    foreach (var procName in new[] { "PRoCon", "PRoCon.Console", "PRoCon.UI" })
                    {
                        foreach (Process proc in Process.GetProcessesByName(procName))
                        {
                            try
                            {
                                string procPath = Path.GetFullPath(proc.MainModule.FileName);
                                if (procPath.StartsWith(baseDir, StringComparison.OrdinalIgnoreCase))
                                {
                                    running = true;
                                    if (waitCounter > 40)
                                    {
                                        Log("Killing PRoCon process...");
                                        proc.Kill();
                                    }
                                }
                            }
                            catch { }
                        }
                    }
                }
                catch { }

                if (!running) break;
                if (waitCounter == 0) Log("Waiting for PRoCon to close...");
                waitCounter++;
                Thread.Sleep(100);
            }

            Log("PRoCon is closed.");
        }

        static void MoveContents(string path)
        {
            if (!Directory.Exists(path)) return;

            foreach (string file in Directory.GetFiles(path))
            {
                string fileName = Path.GetFileName(file);

                if (string.Equals(fileName, "PRoConUpdater.exe", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(fileName, "PRoConUpdater.pdb", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(fileName, "PRoConUpdater.dll", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(fileName, "Ionic.Zip.Reduced.dll", StringComparison.OrdinalIgnoreCase))
                {
                    try { File.Delete(file); } catch { }
                    continue;
                }

                string destination = file.Replace("Updates" + Path.DirectorySeparatorChar, "");

                try
                {
                    string destDir = Path.GetDirectoryName(destination);
                    if (!Directory.Exists(destDir))
                        Directory.CreateDirectory(destDir);
                    if (File.Exists(destination))
                        File.Delete(destination);
                    File.Move(file, destination);
                    Log($"  Updated: {fileName}");
                }
                catch (Exception e)
                {
                    Log($"  Error updating {fileName}: {e.Message}");
                }
            }

            foreach (string dir in Directory.GetDirectories(path))
            {
                MoveContents(dir);
                try { Directory.Delete(dir); } catch { }
            }
        }

        static bool ShouldRestartProcon()
        {
            try
            {
                if (File.Exists("PRoConUpdater.xml"))
                {
                    var doc = new XmlDocument();
                    doc.Load("PRoConUpdater.xml");

                    var optionsList = doc.GetElementsByTagName("options");
                    if (optionsList.Count > 0)
                    {
                        var restartNodes = ((XmlElement)optionsList[0]).GetElementsByTagName("restart");
                        if (restartNodes.Count > 0 && bool.TryParse(restartNodes[0].InnerText, out bool restart))
                            return restart;
                    }
                }
            }
            catch { }

            return true;
        }

        static void Log(string message)
        {
            string line = $"[{DateTime.Now:HH:mm:ss}] {message}";
            Console.WriteLine(line);
            errorLog.AppendLine(line);
        }
    }
}
