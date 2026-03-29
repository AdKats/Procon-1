using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace PRoCon.Core.Logging
{
    /// <summary>
    /// Central static logging facility for PRoCon. Wraps Microsoft.Extensions.Logging
    /// so that all existing FrostbiteConnection.LogError call sites continue to work
    /// while gaining structured logging, log-level filtering, and pluggable providers.
    ///
    /// Call <see cref="Initialize"/> once at startup (e.g. in Program.cs) to wire up
    /// a real ILoggerFactory. Until that happens every logger is a silent no-op, so
    /// the application never crashes due to missing logging configuration.
    /// </summary>
    public static class PRoConLog
    {
        private static ILoggerFactory _factory = NullLoggerFactory.Instance;

        /// <summary>
        /// The shared ILoggerFactory used by the entire application.
        /// Returns <see cref="NullLoggerFactory.Instance"/> when not yet initialized.
        /// </summary>
        public static ILoggerFactory Factory
        {
            get => _factory;
            private set => _factory = value ?? NullLoggerFactory.Instance;
        }

        /// <summary>
        /// Initialize the logging subsystem. Should be called once during startup.
        /// </summary>
        /// <param name="factory">
        /// A configured <see cref="ILoggerFactory"/>. Pass null to reset to no-op logging.
        /// </param>
        public static void Initialize(ILoggerFactory factory)
        {
            Factory = factory;
        }

        /// <summary>
        /// Create a logger for the given category type.
        /// </summary>
        public static ILogger<T> CreateLogger<T>()
        {
            return Factory.CreateLogger<T>();
        }

        /// <summary>
        /// Create a logger for the given category name.
        /// </summary>
        public static ILogger CreateLogger(string categoryName)
        {
            return Factory.CreateLogger(categoryName);
        }

        /// <summary>
        /// Create a logger for the given category type.
        /// </summary>
        public static ILogger CreateLogger(Type type)
        {
            return Factory.CreateLogger(type);
        }
    }
}
