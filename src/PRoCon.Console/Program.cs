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
                    System.Console.WriteLine("Flags:");
                    System.Console.WriteLine("  --rcon-host <ip> --rcon-port <port> --rcon-pass <pass>");
                    System.Console.WriteLine("  --layer-enable --layer-port <port>");
                    System.Console.WriteLine("  --datadir <path>  --interactive / -i");
                    System.Console.WriteLine();

                    application.Execute();

                    GC.Collect();

                    // CLI/env var server connection (one server per launch)
                    string rconHost = GetArg(args, "--rcon-host") ?? Environment.GetEnvironmentVariable("PROCON_RCON_HOST");
                    string rconPortStr = GetArg(args, "--rcon-port") ?? Environment.GetEnvironmentVariable("PROCON_RCON_PORT");
                    string rconPass = GetArg(args, "--rcon-pass") ?? Environment.GetEnvironmentVariable("PROCON_RCON_PASS");

                    if (!string.IsNullOrEmpty(rconHost) && ushort.TryParse(rconPortStr, out ushort rconPort))
                    {
                        System.Console.WriteLine($"Connecting to {rconHost}:{rconPort}...");
                        var client = application.AddConnection(rconHost, rconPort, "default", rconPass ?? "");
                        if (client != null)
                        {
                            client.AutomaticallyConnect = true;

                            // Layer enable (optional)
                            string layerPortStr = GetArg(args, "--layer-port") ?? Environment.GetEnvironmentVariable("PROCON_LAYER_PORT");
                            bool layerEnable = HasFlag(args, "--layer-enable") ||
                                               string.Equals(Environment.GetEnvironmentVariable("PROCON_LAYER_ENABLE"), "true", StringComparison.OrdinalIgnoreCase);
                            if (layerEnable)
                            {
                                ushort layerPort = ushort.TryParse(layerPortStr, out var lp) ? lp : (ushort)27260;
                                client.ProconProtectedLayerEnable(true, layerPort, "0.0.0.0", "PRoCon[%servername%]");
                                System.Console.WriteLine($"Layer enabled on port {layerPort}");
                            }
                        }
                    }

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

        static string GetArg(string[] args, string flag)
        {
            for (int i = 0; i < args.Length - 1; i++)
                if (string.Equals(args[i], flag, StringComparison.OrdinalIgnoreCase))
                    return args[i + 1];
            return null;
        }

        static bool HasFlag(string[] args, string flag)
        {
            for (int i = 0; i < args.Length; i++)
                if (string.Equals(args[i], flag, StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }
    }
}
