/*  Copyright 2010 Geoffrey 'Phogue' Green

    http://www.phogue.net
 
    This file is part of PRoCon Frostbite.

    PRoCon Frostbite is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    PRoCon Frostbite is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with PRoCon Frostbite.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.IO;
using System.Net;
using System.Threading;

namespace PRoCon.Core
{
    public class CDownloadFile
    {

        public delegate void DownloadFileEventDelegate(CDownloadFile sender);
        public event DownloadFileEventDelegate DownloadComplete;
        public event DownloadFileEventDelegate DownloadError;
        public event DownloadFileEventDelegate DownloadDiscoveredFileSize;
        public event DownloadFileEventDelegate DownloadProgressUpdate;

        private HttpWebRequest m_wrRequest;
        private WebResponse m_wrResponse;
        private Stream m_stmResponseStream;

        private string m_strDownloadSource;

        private const int INT_BUFFER_SIZE = UInt16.MaxValue;
        private byte[] ma_bBufferStream;

        private int m_iReadBytes;
        private int m_iCompleteFileSize;
        private byte[] ma_bCompleteFile;

        protected Timer ProgressTick { get; set; }

        private bool m_blFileDownloading;
        public bool FileDownloading
        {
            get
            {
                return this.m_blFileDownloading;
            }
        }

        public int BytesDownloaded
        {
            get
            {
                return this.m_iReadBytes;
            }
        }

        public int FileSize
        {
            get
            {
                return this.m_iCompleteFileSize;
            }
        }

        private double m_dblKibPerSecond = 0.0;
        public double KibPerSecond
        {
            get
            {
                return this.m_dblKibPerSecond;
            }
        }

        public byte[] CompleteFileData
        {
            get
            {
                return this.ma_bCompleteFile;
            }
        }

        private bool m_blUnknownSize;
        public bool UnknownSize
        {
            get
            {
                return this.m_blUnknownSize;
            }
        }

        private object m_objData;
        public object AdditionalData
        {
            get
            {
                return m_objData;
            }
        }

        private string m_strLastError;
        public string Error
        {
            get
            {
                return this.m_strLastError;
            }
        }

        private int m_iTimeout;
        /// <summary>
        /// ReadTimeout of the stream in milliseconds.  Default is 10 seconds.
        /// </summary>
        public int Timeout
        {
            get
            {
                return this.m_iTimeout;
            }
            set
            {
                this.m_iTimeout = value;

                if (this.m_stmResponseStream != null)
                {
                    this.m_stmResponseStream.ReadTimeout = value;
                }
            }
        }

        public string FileName
        {
            get
            {
                string strReturnFileName = String.Empty;

                if (this.m_strDownloadSource.Length > 0)
                {
                    strReturnFileName = this.m_strDownloadSource.Substring(this.m_strDownloadSource.LastIndexOf("/") + 1, (this.m_strDownloadSource.Length - this.m_strDownloadSource.LastIndexOf("/") - 1));
                }

                return strReturnFileName;
            }
        }

        public CDownloadFile(string strDownloadSource)
        {
            this.m_strDownloadSource = strDownloadSource;

            this.m_iTimeout = 10000;
        }

        public CDownloadFile(string strDownloadSource, object objData)
        {
            this.m_strDownloadSource = strDownloadSource;
            this.m_objData = objData;

            this.m_iTimeout = 10000;
        }

        public void EndDownload()
        {
            this.m_blFileDownloading = false;
        }

        public string GetLabelProgress()
        {
            return String.Format("{0:0.0} KiB of {1:0.0} KiB @ {2:0.0} KiB/s", this.m_iReadBytes / 1024, this.m_iCompleteFileSize / 1024, this.m_dblKibPerSecond);
        }

        private void RequestTimeoutCallback(object objState, bool blTimedOut)
        {
            if (blTimedOut == true)
            {
                CDownloadFile cdfParent = (CDownloadFile)objState;
                if (cdfParent != null)
                {
                    cdfParent.m_wrRequest.Abort();
                }
            }
        }

        public void BeginDownload()
        {
            new Thread(new ThreadStart(this.BeginDownloadCallback)).Start();
        }

        private void BeginDownloadCallback()
        {

            this.m_blUnknownSize = true;

            this.m_iReadBytes = 0;
            this.m_iCompleteFileSize = 1;

            this.m_blFileDownloading = true;

            this.ma_bBufferStream = new byte[CDownloadFile.INT_BUFFER_SIZE];

            try
            {
                this.m_wrRequest = (HttpWebRequest)HttpWebRequest.Create(this.m_strDownloadSource);
                this.m_wrRequest.Referer = "http://www.phogue.net/procon/";
                this.m_wrRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                this.m_wrRequest.Headers.Add(System.Net.HttpRequestHeader.AcceptEncoding, "gzip");
                this.m_wrRequest.Proxy = null;
            }
            catch (Exception e) { }

            if (this.m_wrRequest != null)
            {
                IAsyncResult arResult = this.m_wrRequest.BeginGetResponse(new AsyncCallback(this.ResponseCallback), this);
                ThreadPool.RegisterWaitForSingleObject(arResult.AsyncWaitHandle, new WaitOrTimerCallback(this.RequestTimeoutCallback), this, this.m_iTimeout, true);

                this.ProgressTick = new Timer(this.DownloadRateUpdater, this, 0, 100);
            }
        }

        private void ResponseCallback(IAsyncResult ar)
        {
            CDownloadFile cdfParent = (CDownloadFile)ar.AsyncState;

            try
            {
                cdfParent.m_wrResponse = cdfParent.m_wrRequest.EndGetResponse(ar);

                string strContentLength = null;
                if ((strContentLength = cdfParent.m_wrResponse.Headers["Content-Length"]) != null)
                {
                    cdfParent.m_iCompleteFileSize = Convert.ToInt32(strContentLength);
                    cdfParent.ma_bCompleteFile = new byte[cdfParent.m_iCompleteFileSize];

                    cdfParent.m_blUnknownSize = false;

                    if (cdfParent.DownloadDiscoveredFileSize != null)
                    {
                        //cdfParent.DownloadDiscoveredFileSize(cdfParent);

                        cdfParent.DownloadDiscoveredFileSize(cdfParent);
                    }
                }

                cdfParent.ma_bCompleteFile = new byte[0];

                cdfParent.m_stmResponseStream = cdfParent.m_wrResponse.GetResponseStream();

                IAsyncResult arResult = cdfParent.m_stmResponseStream.BeginRead(cdfParent.ma_bBufferStream, 0, CDownloadFile.INT_BUFFER_SIZE, new AsyncCallback(cdfParent.ReadCallBack), cdfParent);

                ThreadPool.RegisterWaitForSingleObject(arResult.AsyncWaitHandle, new WaitOrTimerCallback(cdfParent.ReadTimeoutCallback), cdfParent, cdfParent.m_iTimeout, true);
            }
            catch (Exception e)
            {
                cdfParent.m_blFileDownloading = false;
                if (cdfParent.DownloadError != null)
                {
                    cdfParent.m_strLastError = e.Message;

                    cdfParent.DownloadError(cdfParent);
                    //cdfParent.DownloadError(cdfParent);
                }
            }
        }

        private void ReadTimeoutCallback(object objState, bool blTimedOut)
        {
            if (blTimedOut == true)
            {
                CDownloadFile cdfParent = (CDownloadFile)objState;
                if (cdfParent != null && cdfParent.m_stmResponseStream != null)
                {
                    cdfParent.m_stmResponseStream.Close();

                    if (cdfParent.DownloadError != null)
                    {
                        cdfParent.m_strLastError = "Read Timeout";

                        cdfParent.DownloadError(cdfParent);
                        //cdfParent.DownloadError(cdfParent);
                    }
                }
            }
        }

        private void ReadCallBack(IAsyncResult ar)
        {
            CDownloadFile cdfParent = (CDownloadFile)ar.AsyncState;

            if (cdfParent.m_blFileDownloading == true)
            {
                try
                {

                    int iBytesRead = -1;
                    if ((iBytesRead = cdfParent.m_stmResponseStream.EndRead(ar)) > 0)
                    {


                        if (this.CompleteFileData.Length < this.BytesDownloaded + iBytesRead)
                        {
                            byte[] resizedFileData = new byte[this.CompleteFileData.Length + iBytesRead];

                            this.CompleteFileData.CopyTo(resizedFileData, 0);

                            this.ma_bCompleteFile = resizedFileData;
                        }

                        Array.Copy(this.ma_bBufferStream, 0, this.CompleteFileData, this.BytesDownloaded, iBytesRead);
                        cdfParent.m_iReadBytes += iBytesRead;

                        IAsyncResult arResult = cdfParent.m_stmResponseStream.BeginRead(cdfParent.ma_bBufferStream, 0, CDownloadFile.INT_BUFFER_SIZE, new AsyncCallback(cdfParent.ReadCallBack), cdfParent);

                        ThreadPool.RegisterWaitForSingleObject(arResult.AsyncWaitHandle, new WaitOrTimerCallback(cdfParent.ReadTimeoutCallback), cdfParent, cdfParent.m_iTimeout, true);
                    }
                    else
                    {

                        cdfParent.m_blFileDownloading = false;
                        if (cdfParent.DownloadComplete != null)
                        {
                            cdfParent.DownloadComplete(cdfParent);
                            //cdfParent.DownloadComplete(cdfParent);
                        }

                        cdfParent.m_stmResponseStream.Close();
                        cdfParent.m_stmResponseStream.Dispose();
                        cdfParent.m_stmResponseStream = null;
                    }
                }
                catch (Exception e)
                {
                    cdfParent.m_blFileDownloading = false;
                    if (cdfParent.DownloadError != null)
                    {
                        cdfParent.m_strLastError = e.Message;

                        cdfParent.DownloadError(cdfParent);
                        //cdfParent.DownloadError(cdfParent);
                    }
                }
            }
        }

        private int iTickCount = 0;
        private int[] a_iKiBytesPerTick = new int[50];
        private int iPreviousTickReadBytes = 0;

        private void DownloadRateUpdater(Object obj)
        {

            CDownloadFile cdfParent = ((CDownloadFile)obj);

            if (((CDownloadFile)obj).FileDownloading == true)
            {

                a_iKiBytesPerTick[iTickCount] = cdfParent.m_iReadBytes - iPreviousTickReadBytes;
                iTickCount = (++iTickCount % 50);

                cdfParent.m_dblKibPerSecond = 0.0;
                foreach (int iKiBytesTick in a_iKiBytesPerTick)
                {
                    cdfParent.m_dblKibPerSecond += iKiBytesTick;
                }

                cdfParent.m_dblKibPerSecond = cdfParent.m_dblKibPerSecond / 5120; // / 1024 / 5;

                iPreviousTickReadBytes = cdfParent.m_iReadBytes;

                if (cdfParent.DownloadProgressUpdate != null && iPreviousTickReadBytes > 0)
                {
                    //cdfParent.DownloadProgressUpdate(cdfParent);

                    cdfParent.DownloadProgressUpdate(cdfParent);
                }
            }
            else
            {
                // Don't run again.
                this.ProgressTick.Dispose();
            }
        }
    }
}
