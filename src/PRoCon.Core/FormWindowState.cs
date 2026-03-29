// Cross-platform replacements for System.Windows.Forms types
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

    /// <summary>
    /// Cross-platform replacement for System.Drawing.Rectangle.
    /// Stores window position and size for config persistence.
    /// </summary>
    public struct WindowBounds
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;

        public WindowBounds(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }
}
