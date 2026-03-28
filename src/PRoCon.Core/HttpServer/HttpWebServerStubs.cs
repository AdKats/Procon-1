using System;
using System.Collections.Specialized;

namespace PRoCon.Core.HttpServer
{
    /// <summary>
    /// Stub retained for plugin backward compatibility.
    /// The HTTP web server was removed in v2.0 (replaced by SignalR).
    /// </summary>
    [Obsolete("HTTP web server removed in v2.0. This type exists only for plugin compilation compatibility.")]
    [Serializable]
    public class HttpWebServerRequestData
    {
        public string Method { get; set; } = "";
        public string Request { get; set; } = "";
        public string RequestPath { get; set; } = "";
        public string RequestFile { get; set; } = "";
        public NameValueCollection Query { get; set; } = new NameValueCollection();
        public string HttpVersion { get; set; } = "1.1";
        public string Post { get; set; } = "";

        public HttpWebServerRequestData() { }
        public HttpWebServerRequestData(string document) { }
    }

    /// <summary>
    /// Stub retained for plugin backward compatibility.
    /// The HTTP web server was removed in v2.0 (replaced by SignalR).
    /// </summary>
    [Obsolete("HTTP web server removed in v2.0. This type exists only for plugin compilation compatibility.")]
    [Serializable]
    public class HttpWebServerResponseData
    {
        public string HttpVersion { get; set; } = "1.1";
        public string StatusCode { get; set; } = "200 OK";
        public string Document { get; set; } = "";

        public HttpWebServerResponseData(string document)
        {
            Document = document;
        }

        public override string ToString() => Document;
    }
}
