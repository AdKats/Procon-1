using System;

namespace PRoCon.Core.Remote
{
    /// <summary>
    /// Tracks connection health for a single server and recommends adaptive polling intervals.
    /// Default: 5s. Backs off to 30s when the server is lagging. Recovers when healthy.
    /// </summary>
    public class ConnectionHealthTracker
    {
        public static readonly TimeSpan MinInterval = TimeSpan.FromSeconds(5);
        public static readonly TimeSpan MaxInterval = TimeSpan.FromSeconds(30);

        private const int SlowTicksBeforeBackoff = 2;
        private const int FastTicksBeforeSpeedup = 3;
        private const double BackoffMultiplier = 1.5;
        private const double SpeedupMultiplier = 0.75;

        public TimeSpan CurrentInterval { get; private set; } = MinInterval;
        public DateTime LastPollTime { get; set; } = DateTime.MinValue;

        private int _consecutiveSlowTicks;
        private int _consecutiveFastTicks;

        /// <summary>
        /// Evaluates the connection health and returns the recommended polling interval.
        /// Call this on each master-tick when this server is due for a poll.
        /// </summary>
        public TimeSpan Evaluate(FrostbiteConnection connection)
        {
            if (connection == null)
            {
                CurrentInterval = MaxInterval;
                return CurrentInterval;
            }

            bool isLagging = false;

            // Check packet queue backlog
            if (connection.OutgoingPacketCount > 2)
                isLagging = true;
            else if (connection.QueuedPacketCount > 5)
                isLagging = true;
            else if (connection.OldestOutgoingPacketAge.TotalSeconds > 3)
                isLagging = true;

            // Check if we haven't received any packet in a while
            if (connection.LastPacketReceived != null &&
                (DateTime.Now - connection.LastPacketReceived.Stamp).TotalSeconds > 10)
                isLagging = true;

            if (isLagging)
            {
                _consecutiveFastTicks = 0;
                _consecutiveSlowTicks++;

                if (_consecutiveSlowTicks >= SlowTicksBeforeBackoff)
                {
                    double newSeconds = CurrentInterval.TotalSeconds * BackoffMultiplier;
                    CurrentInterval = TimeSpan.FromSeconds(Math.Min(newSeconds, MaxInterval.TotalSeconds));
                }
            }
            else
            {
                _consecutiveSlowTicks = 0;
                _consecutiveFastTicks++;

                if (_consecutiveFastTicks >= FastTicksBeforeSpeedup)
                {
                    double newSeconds = CurrentInterval.TotalSeconds * SpeedupMultiplier;
                    CurrentInterval = TimeSpan.FromSeconds(Math.Max(newSeconds, MinInterval.TotalSeconds));
                }
            }

            return CurrentInterval;
        }

        /// <summary>
        /// Resets the tracker to default 5-second interval. Call on reconnect.
        /// </summary>
        public void Reset()
        {
            CurrentInterval = MinInterval;
            LastPollTime = DateTime.MinValue;
            _consecutiveSlowTicks = 0;
            _consecutiveFastTicks = 0;
        }
    }
}
