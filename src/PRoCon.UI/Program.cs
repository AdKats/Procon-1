using System;
using System.Threading;
using Avalonia;
using Avalonia.ReactiveUI;
using PRoCon.Core;

namespace PRoCon.UI
{
    class Program
    {
        private static Mutex _singleInstanceMutex;

        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                RunApp(args);
            }
            catch (Exception ex)
            {
                string crashPath = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    "procon-crash.log");
                try { System.IO.File.WriteAllText(crashPath, $"PRoCon crashed on startup:\n{ex}"); } catch { }
                throw;
            }
        }

        private static void RunApp(string[] args)
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

            // Use data directory in mutex name so separate installs can run simultaneously
            string baseDir = ProConPaths.DataDirectory;
            // Simple FNV-1a hash for stable cross-run results
            uint hash = 2166136261;
            foreach (char c in baseDir)
            {
                hash ^= c;
                hash *= 16777619;
            }
            string mutexName = $"Global\\PRoCon_v2_{hash:X8}";

            _singleInstanceMutex = new Mutex(true, mutexName, out bool createdNew);

            if (!createdNew)
            {
                Console.Error.WriteLine("Another PRoCon instance is running from this directory.");

                // Try to acquire the mutex
                try
                {
                    if (!_singleInstanceMutex.WaitOne(2000))
                    {
                        Console.Error.WriteLine("Could not acquire mutex. Forcing launch anyway.");
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
                try { _singleInstanceMutex.ReleaseMutex(); } catch { }
                _singleInstanceMutex.Dispose();
            }
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .UseReactiveUI();
    }
}
