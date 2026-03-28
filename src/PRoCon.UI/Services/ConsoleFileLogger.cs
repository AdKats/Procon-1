using System;
using System.IO;

namespace PRoCon.UI.Services
{
    public class ConsoleFileLogger : IDisposable
    {
        private readonly string _logDirectory;
        private readonly string _baseFileName;
        private readonly long _maxFileSize;
        private readonly int _maxFiles;
        private readonly object _lock = new object();
        private StreamWriter _writer;
        private string _currentFilePath;
        private long _currentSize;
        private bool _disposed;

        public ConsoleFileLogger(string logDirectory, string baseFileName = "rcon", long maxFileSize = 1048576, int maxFiles = 5)
        {
            _logDirectory = logDirectory;
            _baseFileName = baseFileName;
            _maxFileSize = maxFileSize;
            _maxFiles = maxFiles;

            Directory.CreateDirectory(_logDirectory);
            _currentFilePath = Path.Combine(_logDirectory, $"{_baseFileName}.log");
            OpenWriter();
        }

        public void WriteLine(string line)
        {
            if (_disposed) return;
            lock (_lock)
            {
                if (_disposed) return;
                try
                {
                    RotateIfNeeded();
                    _writer?.WriteLine(line);
                    _writer?.Flush();
                    _currentSize += System.Text.Encoding.UTF8.GetByteCount(line) + Environment.NewLine.Length;
                }
                catch
                {
                    // Swallow I/O errors to avoid crashing the UI
                }
            }
        }

        private void RotateIfNeeded()
        {
            if (_currentSize < _maxFileSize) return;

            CloseWriter();

            // Shift existing rotated files: rcon.4.log -> rcon.5.log (deleted), ..., rcon.1.log -> rcon.2.log
            for (int i = _maxFiles; i >= 1; i--)
            {
                string source = Path.Combine(_logDirectory, $"{_baseFileName}.{i}.log");
                if (i == _maxFiles)
                {
                    try { File.Delete(source); } catch { }
                }
                else
                {
                    string dest = Path.Combine(_logDirectory, $"{_baseFileName}.{i + 1}.log");
                    try
                    {
                        if (File.Exists(source))
                            File.Move(source, dest, true);
                    }
                    catch { }
                }
            }

            // Move current log to .1.log
            string rotatedPath = Path.Combine(_logDirectory, $"{_baseFileName}.1.log");
            try
            {
                if (File.Exists(_currentFilePath))
                    File.Move(_currentFilePath, rotatedPath, true);
            }
            catch { }

            OpenWriter();
        }

        private void OpenWriter()
        {
            try
            {
                _writer = new StreamWriter(_currentFilePath, append: true, encoding: System.Text.Encoding.UTF8);
                _currentSize = new FileInfo(_currentFilePath).Exists ? new FileInfo(_currentFilePath).Length : 0;
            }
            catch
            {
                _writer = null;
                _currentSize = 0;
            }
        }

        private void CloseWriter()
        {
            try
            {
                _writer?.Flush();
                _writer?.Dispose();
            }
            catch { }
            _writer = null;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            lock (_lock)
            {
                CloseWriter();
            }
        }
    }
}
