using System;
using System.ComponentModel;

namespace PRoCon
{
    public static class ControlHelpers
    {
        private readonly static object[] Empty = new object[0];

        public static void InvokeIfRequired(this ISynchronizeInvoke control, Action action)
        {
            try
            {
                if (control.InvokeRequired)
                {
                    control.Invoke(action, Empty);
                }
                else
                {
                    action();
                }
            }
            // Suppress object disposed which can occur while we are shutting down.
            catch (ObjectDisposedException) { }
            catch (InvalidOperationException) { }
            // This particular catch fixes #115
            catch (InvalidAsynchronousStateException) { }
            // Suppress all the exceptions!
            // This is here simply because there is far to much legacy code to go through.
            catch { }
        }
    }
}