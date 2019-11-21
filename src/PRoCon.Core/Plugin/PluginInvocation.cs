using System;
using System.Text;

namespace PRoCon.Core.Plugin
{

    /// <summary>
    /// Just a class to stamp a call to a plugin.
    /// 
    /// This class is used to store details on a call to a plugin
    /// with a stamp which can then be used to work out how long a
    /// a call has taken.
    /// </summary>
    public class PluginInvocation
    {

        /// <summary>
        /// The maxmimum time a plugin invocation should be allowed to execute
        /// 
        /// Default to 10 seconds.
        /// </summary>
        public static TimeSpan MaximumRuntime = new TimeSpan(0, 0, 10);

        /// <summary>
        /// When this class was instantiated
        /// </summary>
        public DateTime Stamp { get; protected set; }

        /// <summary>
        /// The target plugin of the invocation
        /// </summary>
        public Plugin Plugin { get; set; }

        /// <summary>
        /// The specific method to call.
        /// </summary>
        public String MethodName { get; set; }

        /// <summary>
        /// The parameters being passed to the method.
        /// </summary>
        public Object[] Parameters { get; set; }

        public PluginInvocation()
        {
            this.Reset();
        }

        /// <summary>
        /// Resets the stamp of this invocation (Runtime == 0)
        /// </summary>
        /// <returns>Itself</returns>
        public PluginInvocation Reset()
        {
            this.Stamp = DateTime.Now;

            return this;
        }

        /// <summary>
        /// Fetches how long the invocation has been running.
        /// </summary>
        /// <returns>The length of time this plugin invocation has been running for.</returns>
        public TimeSpan Runtime()
        {
            return DateTime.Now - this.Stamp;
        }

        /// <summary>
        /// Formats this invocation as a fault with an optional error.
        /// </summary>
        /// <returns>Returns the formatted fault output for logging</returns>
        public string FormatInvocationFault(String format = null, params object[] parameters)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("Fault in plugin call to {0}.{1}", this.Plugin.ClassName, this.MethodName);
            sb.AppendLine();

            if (format != null)
            {
                sb.AppendFormat(format, parameters);
                sb.AppendLine();
            }

            for (int i = 0; i < this.Parameters.Length; i++)
            {
                sb.AppendFormat("\tParameter {0}: {1}, value: \"{2}\"", i, this.Parameters[i].GetType(), this.Parameters[i].ToString());
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public override string ToString()
        {
            return this.FormatInvocationFault();
        }
    }
}
