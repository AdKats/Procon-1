// Cross-platform replacement for System.Windows.Forms.FormWindowState
// Used to persist and restore window state without depending on WinForms.

namespace PRoCon.Core
{
    /// <summary>
    /// Specifies how a form window is displayed.
    /// Mirrors System.Windows.Forms.FormWindowState values for serialization compatibility.
    /// </summary>
    public enum FormWindowState
    {
        Normal = 0,
        Minimized = 1,
        Maximized = 2
    }
}
