using System;
using System.Net.Sockets;
using System.Threading;
using PRoCon.Core;
using PRoCon.Core.Remote;

namespace PRoCon.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            // Handle --datadir before anything else
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (string.Equals(args[i], "--datadir", StringComparison.OrdinalIgnoreCase))
                {
                    ProConPaths.SetDataDirectory(args[i + 1]);
                    break;
                }
            }

            int connectionInterrupts = 0;
            int maxConnectionInterrupts = 5;

            if (args != null && args.Length >= 2)
            {
                for (int i = 0; i < args.Length; i = i + 2)
                {
                    int iValue;
                    if (String.Compare("-use_core", args[i], true) == 0 && int.TryParse(args[i + 1], out iValue) && iValue > 0)
                    {
                        System.Diagnostics.Process.GetCurrentProcess().ProcessorAffinity = (IntPtr)iValue;
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

                    System.Console.WriteLine("PRoCon Frostbite v2.0");
                    System.Console.WriteLine("=====================");
                    System.Console.WriteLine("Headless console mode for servers and containers.");
                    System.Console.WriteLine($"Data directory: {ProConPaths.DataDirectory}");
                    if (ProConPaths.IsContainer)
                        System.Console.WriteLine("Container detected — using /config/");
                    System.Console.WriteLine();

                    application.Execute();

                    GC.Collect();

                    // Game server health monitoring (optional, set via env vars)
                    string gameServerIP = Environment.GetEnvironmentVariable("PROCON_GAMESERVER_IP") ?? "";
                    if (gameServerIP != "")
                    {
                        Int32.TryParse(Environment.GetEnvironmentVariable("PROCON_GAMESERVER_PORT"), out int gameServerPort);

                        Thread healthThread = new Thread(() =>
                        {
                            while (true)
                            {
                                Thread.Sleep(60000);
                                string ts = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                                using (TcpClient tcpClient = new TcpClient())
                                {
                                    try
                                    {
                                        tcpClient.Connect(gameServerIP, gameServerPort);
                                        tcpClient.Close();

                                        if (connectionInterrupts > 0)
                                        {
                                            System.Console.WriteLine($"[{ts}] Game server reconnected after {connectionInterrupts} failed attempt(s).");
                                            connectionInterrupts = 0;
                                        }
                                    }
                                    catch
                                    {
                                        connectionInterrupts++;
                                        System.Console.WriteLine($"[{ts}] Connection check failed ({connectionInterrupts}/{maxConnectionInterrupts}).");

                                        if (connectionInterrupts >= maxConnectionInterrupts)
                                        {
                                            System.Console.WriteLine($"[{ts}] Connection lost. Shutting down.");
                                            application.Shutdown();
                                            application = null;
                                            exitEvent.Set();
                                            return;
                                        }
                                    }
                                }
                            }
                        });
                        healthThread.IsBackground = true;
                        healthThread.Start();
                    }

                    // Graceful shutdown handlers
                    PRoConApplication shutdownApp = application;
                    System.Console.CancelKeyPress += (sender, e) =>
                    {
                        e.Cancel = true;
                        System.Console.WriteLine("Received shutdown signal. Shutting down...");
                        shutdownApp?.Shutdown();
                        application = null;
                        exitEvent.Set();
                    };

                    AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
                    {
                        shutdownApp?.Shutdown();
                    };

                    System.Console.WriteLine("Running... (Ctrl+C or SIGTERM to shutdown)");
                    exitEvent.WaitOne();
                }
                catch (Exception e)
                {
                    FrostbiteConnection.LogError("PRoCon.Console", "", e);
                }
                finally
                {
                    application?.Shutdown();
                }
            }
            else
            {
                System.Console.WriteLine("Already running — shutting down.");
                Thread.Sleep(50);
            }
        }
    }
}
