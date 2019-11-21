using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace PRoCon.Core.Logging
{
    using Core.Remote;
    public class Loggable
    {

        public delegate void WriteConsoleHandler(DateTime dtLoggedTime, string strLoggedText);

        private FileStream m_stmFile;
        private StreamWriter m_stwFileWriter;

        private static Regex RemoveCaretCodes = new Regex(@"\^[0-9]|\^b|\^i|\^n", RegexOptions.Compiled);

        public Loggable()
        {
            this.FileHostNamePort = String.Empty;
            this.LoggingStartedPrefix = "logging started";
            this.LoggingStoppedPrefix = "logging stopped";
            this.FileNameSuffix = String.Empty;

            //this.RemoveCaretCodes 
        }

        protected string FileHostNamePort
        {
            get;
            set;
        }

        protected string LoggingStartedPrefix
        {
            get;
            set;
        }

        protected string LoggingStoppedPrefix
        {
            get;
            set;
        }

        protected string FileNameSuffix
        {
            get;
            set;
        }

        private bool m_blLogging;
        public bool Logging
        {
            get
            {
                return m_blLogging;
            }
            set
            {

                if (value != this.m_blLogging)
                {

                    if (value == true)
                    {

                        try
                        {

                            if (Directory.Exists(Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs"), this.FileHostNamePort)) == false)
                            {
                                Directory.CreateDirectory(Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs"), this.FileHostNamePort));
                            }

                            if (this.m_stmFile == null)
                            {
                                this.m_blLogging = true;

                                if ((this.m_stmFile = new FileStream(Path.Combine(Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs"), this.FileHostNamePort), DateTime.Now.ToString("yyyyMMdd") + "_" + this.FileNameSuffix + ".log"), FileMode.Append)) != null)
                                {
                                    if ((this.m_stwFileWriter = new StreamWriter(this.m_stmFile, Encoding.UTF8)) != null)
                                    {

                                        this.WriteLogLine("{0}: {1}", this.LoggingStartedPrefix, DateTime.Now.ToString("dddd, d MMMM yyyy HH:mm:ss"));
                                    }
                                }
                            }
                        }
                        catch (Exception)
                        {
                            this.m_blLogging = false;
                        }
                    }
                    else
                    {
                        if (this.m_stwFileWriter != null)
                        {

                            this.WriteLogLine("{0}: {1}", this.LoggingStoppedPrefix, DateTime.Now.ToString("dddd, d MMMM yyyy HH:mm:ss"));

                            this.m_stwFileWriter.Close();
                            this.m_stwFileWriter.Dispose();
                            this.m_stwFileWriter = null;
                        }

                        if (this.m_stmFile != null)
                        {
                            this.m_stmFile.Close();
                            this.m_stmFile.Dispose();
                            this.m_stmFile = null;
                        }

                        this.m_blLogging = false;
                    }
                }
            }
        }

        protected void WriteLogLine(string strFormat, params object[] a_objArguments)
        {
            if (this.Logging == true && this.m_stwFileWriter != null)
            {
                try
                {
                    this.m_stwFileWriter.WriteLine(Loggable.RemoveCaretCodes.Replace(String.Format(strFormat, a_objArguments), ""));
                    this.m_stwFileWriter.Flush();
                }
                catch (Exception e)
                {
                    FrostbiteConnection.LogError("WriteLogLine error", strFormat, e);
                }
            }
        }
    }
}
