using Avalonia;
using Avalonia.ReactiveUI;
using System;
using System.Threading;

namespace PRoCon.UI
{
    class Program
    {
        private static Mutex _singleInstanceMutex;

        [STAThread]
        public static void Main(string[] args)
        {
            const string mutexName = "Global\\PRoCon_Frostbite_v2_SingleInstance";

            _singleInstanceMutex = new Mutex(true, mutexName, out bool createdNew);

            if (!createdNew)
            {
                Console.Error.WriteLine("PRoCon is already running. Closing previous instance...");

                // Kill any existing PRoCon.UI processes (except ourselves)
                int currentPid = System.Diagnostics.Process.GetCurrentProcess().Id;
                foreach (var proc in System.Diagnostics.Process.GetProcessesByName("PRoCon.UI"))
                {
                    try
                    {
                        if (proc.Id != currentPid)
                        {
                            proc.Kill();
                            proc.WaitForExit(3000);
                        }
                    }
                    catch { }
                }

                // Also check for dotnet-hosted instances
                foreach (var proc in System.Diagnostics.Process.GetProcessesByName("dotnet"))
                {
                    try
                    {
                        if (proc.Id != currentPid && proc.MainModule?.FileName != null)
                        {
                            // Only kill if it's running our UI
                            string cmdLine = proc.MainModule.FileName;
                            // Can't easily check args, skip dotnet processes
                        }
                    }
                    catch { }
                }

                // Try to acquire the mutex now
                try
                {
                    if (!_singleInstanceMutex.WaitOne(2000))
                    {
                        Console.Error.WriteLine("Could not acquire mutex. Forcing launch anyway.");
                        // Dispose and recreate
                        _singleInstanceMutex.Dispose();
                        _singleInstanceMutex = new Mutex(true, mutexName, out createdNew);
                    }
                }
                catch (AbandonedMutexException)
                {
                    // Previous holder crashed — we now own the mutex
                }
            }

            try
            {
                BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            }
            finally
            {
                _singleInstanceMutex.ReleaseMutex();
                _singleInstanceMutex.Dispose();
            }
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace()
                .UseReactiveUI();
    }
}
