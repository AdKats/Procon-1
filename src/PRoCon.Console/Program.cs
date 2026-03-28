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
using System.Threading;

namespace PRoCon.Console
{
    using Core;
    using Core.Remote;
    using System.Net.Sockets;

    class Program
    {


        static void Main(string[] args)
        {
            int connectionIntrupts = 0;

            int maxConnectionIntruppts = 5;

            int iValue;

            if (args != null && args.Length >= 2)
            {
                for (int i = 0; i < args.Length; i = i + 2)
                {
                    if (String.Compare("-use_core", args[i], true) == 0 && int.TryParse(args[i + 1], out iValue) == true && iValue > 0)
                    {
                        System.Diagnostics.Process.GetCurrentProcess().ProcessorAffinity = (System.IntPtr)iValue;
                    }
                }
            }

            PRoConApplication application = null;

            if (PRoConApplication.IsProcessOpen() == false)
            {
                var exitEvent = new ManualResetEvent(false);

                try
                {
                    application = new PRoConApplication(true, args);

                    System.Console.WriteLine("Procon Frostbite");
                    System.Console.WriteLine("================");
                    System.Console.WriteLine("By executing this application you agree to the license available at:");
                    System.Console.WriteLine("\thttps://myrcon.net/licenses/myrcon.pdf");
                    System.Console.WriteLine("If you do not agree you must immediately exit this application.");
                    System.Console.WriteLine("================");
                    System.Console.WriteLine("This is a cut down version of PRoCon.exe to be used by GSPs and PRoCon Hosts.");
                    System.Console.WriteLine("Executing this file is the same as \"PRoCon.exe -console 1\"");
                    System.Console.WriteLine("No output is given.  This is as cut down as we're gunno get..");
                    System.Console.WriteLine("\nExecuting procon...");
                    application.Execute();

                    GC.Collect();

                    // Check if we are running in a container
                    bool isContainer = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true"
                        || System.IO.File.Exists("/.dockerenv");
                    if (!isContainer && System.IO.File.Exists("/proc/1/cgroup"))
                    {
                        string strCGroup = System.IO.File.ReadAllText("/proc/1/cgroup");
                        isContainer = strCGroup.Contains("/docker/") || strCGroup.Contains("/kubepods/");
                    }
                    if (isContainer)
                    {
                        System.Console.WriteLine("[PRoCon] Running in a container. Auto-updater disabled.");
                        // Auto-updater is disabled in container mode — containers are immutable
                    }

                    // Check if the environemnt variable "PROCON_GAMESERVER_IP" exists
                    string PROCON_GAMESERVER_IP = System.Environment.GetEnvironmentVariable("PROCON_GAMESERVER_IP") ?? "";

                    if (PROCON_GAMESERVER_IP != "")
                    {
                        // Run a background thread to keep checking if the connection is still alive, otherwise close application.
                        Thread t = new Thread(new ThreadStart(delegate
                        {
                            Int32.TryParse(System.Environment.GetEnvironmentVariable("PROCON_GAMESERVER_PORT"), out int PROCON_GAMESERVER_PORT);

                            while (true)
                            {
                                Thread.Sleep(60000);

                                string currentTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                                // Check if port is alive using the ip PROCON_GAMESERVER_IP and port PROCON_GAMESERVER_PORT
                                using (TcpClient tcpClient = new TcpClient())
                                {
                                    try
                                    {
                                        tcpClient.Connect(PROCON_GAMESERVER_IP, PROCON_GAMESERVER_PORT);
                                        tcpClient.Close();

                                        // Reset counter only after a successful connection
                                        if (connectionIntrupts > 0)
                                        {
                                            System.Console.WriteLine("[" + currentTimestamp + "] [PRoCon] Game server reconnected after " + connectionIntrupts + " failed attempt(s).");
                                            connectionIntrupts = 0;
                                        }

                                        System.Console.WriteLine("[" + currentTimestamp + "] Game server connection successful.");
                                    }
                                    catch (Exception)
                                    {
                                        connectionIntrupts++;

                                        System.Console.WriteLine("[" + currentTimestamp + "] [PRoCon] Connection check failed (" + connectionIntrupts + "/" + maxConnectionIntruppts + ").");

                                        // Once we reach the max amount of connection attempts, shut down gracefully.
                                        if (connectionIntrupts >= maxConnectionIntruppts)
                                        {
                                            System.Console.WriteLine("[" + currentTimestamp + "] [PRoCon] Connection lost after " + connectionIntrupts + " attempts. Shutting down.");
                                            application.Shutdown();
                                            application = null; // Prevent double shutdown in finally block
                                            exitEvent.Set();
                                            return;
                                        }
                                    }
                                }
                            }
                        }));

                        t.IsBackground = true;
                        t.Start();
                    }

                    // Register signal handlers for graceful shutdown (works in Docker without a TTY)
                    PRoConApplication shutdownApp = application;
                    System.Console.CancelKeyPress += (sender, e) =>
                    {
                        e.Cancel = true;
                        System.Console.WriteLine("[PRoCon] Received shutdown signal (SIGINT). Shutting down gracefully...");
                        if (shutdownApp != null) shutdownApp.Shutdown();
                        application = null; // Prevent double shutdown in finally block
                        exitEvent.Set();
                    };

                    // ProcessExit has a ~2 second time limit on .NET Framework/Mono
                    AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
                    {
                        System.Console.WriteLine("[PRoCon] Process exit. Shutting down gracefully...");
                        if (shutdownApp != null) shutdownApp.Shutdown();
                    };

                    System.Console.WriteLine("Running... (Send SIGINT or SIGTERM to shutdown)");
                    exitEvent.WaitOne();
                }
                catch (Exception e)
                {
                    FrostbiteConnection.LogError("PRoCon.Console.exe", "", e);
                }
                finally
                {
                    if (application != null)
                    {
                        application.Shutdown();
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
                System.Console.WriteLine("Already running - shutting down");
                Thread.Sleep(50);
            }

        }
    }
}
