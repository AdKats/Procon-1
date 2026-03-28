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
            // Use base directory in mutex name so separate installs can run simultaneously
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
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
