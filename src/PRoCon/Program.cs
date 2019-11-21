using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace PRoCon
{
    using Core;
    using Core.AutoUpdates;
    using Core.Remote;
    using Forms;

    public static class Program
    {

        // This is a simple test for me to look into TFS's source control stuff..

        public static PRoConApplication ProconApplication;
        public static string[] Args;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {

            Program.Args = args;

            bool dotNetCheck = CheckNetVersion("3.5");

            if (PRoConApplication.IsProcessOpen() == false && dotNetCheck == true)
            {

                if (Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Updates")) == true)
                {
                    AutoUpdater.Arguments = args;
                    AutoUpdater.BeginUpdateProcess(null);
                }
                else
                {
                    try
                    {

                        bool isBasicConsole = false;
                        bool isGspUpdater = false;

                        if (args != null && args.Length >= 2)
                        {
                            for (int i = 0; i < args.Length; i = i + 2)
                            {
                                int value;

                                if (String.Compare("-console", args[i], System.StringComparison.OrdinalIgnoreCase) == 0 && int.TryParse(args[i + 1], out value) == true && value == 1)
                                {
                                    isBasicConsole = true;
                                }
                                if (String.Compare("-gspupdater", args[i], System.StringComparison.OrdinalIgnoreCase) == 0 && int.TryParse(args[i + 1], out value) == true && value == 1)
                                {
                                    isGspUpdater = true;
                                }
                                if (String.Compare("-use_core", args[i], System.StringComparison.OrdinalIgnoreCase) == 0 && int.TryParse(args[i + 1], out value) == true && value > 0)
                                {
                                    System.Diagnostics.Process.GetCurrentProcess().ProcessorAffinity = (System.IntPtr)value;
                                }
                            }
                        }

                        Application.EnableVisualStyles();
                        Application.SetCompatibleTextRenderingDefault(false);

                        if (isGspUpdater == true)
                        {
                            Application.Run(new GspUpdater());
                        }
                        else
                        {

                            if (isBasicConsole == true)
                            {
                                BasicConsole basicWindow = new BasicConsole();
                                basicWindow.WindowLoaded += new BasicConsole.WindowLoadedHandler(procon_WindowLoaded);

                                Application.Run(basicWindow);

                            }
                            else
                            {
                                frmMain mainWindow = new frmMain(args);
                                mainWindow.WindowLoaded += new frmMain.WindowLoadedHandler(procon_WindowLoaded);
                                Application.Run(mainWindow);
                            }

                        }
                    }
                    catch (Exception e)
                    {
                        FrostbiteConnection.LogError("Application error", String.Empty, e);
                        MessageBox.Show("Procon ran into a critical error, but hopefully it logged that error in DEBUG.txt. Please post this to the forums at https://myrcon.net/");
                    }
                    finally
                    {
                        if (Program.ProconApplication != null)
                        {
                            Program.ProconApplication.Shutdown();
                        }
                    }
                }
            }
            else
            {
                // Possible prevention of a cpu consumption bug I can see at the time of writing.
                // TCAdmin: Start procon.exe
                // procon.exe has an update to install
                // procon.exe loads proconupdater.exe
                // procon.exe unloads
                // proconupdater.exe begins update
                // TCAdmin detects procon.exe shutdown - attempts to reload
                Thread.Sleep(50);
            }
        }

        static PRoConApplication procon_WindowLoaded(bool execute)
        {
            Program.ProconApplication = new PRoConApplication(false, Program.Args);

            if (execute == true)
            {
                Program.ProconApplication.Execute();
            }

            return Program.ProconApplication;
        }

        private static bool CheckNetVersion(string sExpectedVersion)
        {

            bool neededNetFound = false;

            string neededVersion = "v" + sExpectedVersion;

            RegistryKey installedVersions = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP");

            if (installedVersions != null)
            {
                string[] versionNames = installedVersions.GetSubKeyNames();
                installedVersions.Close();

                // versions include v
                if (versionNames.Any(t => t.IndexOf(neededVersion, System.StringComparison.Ordinal) > -1))
                {
                    neededNetFound = true;
                }

                if (neededNetFound == false)
                {
                    MessageBox.Show("You need at least .NET " + sExpectedVersion + " installed!", "Procon Frostbite .NET Check", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }
            }

            return true;
        }

    }

}
