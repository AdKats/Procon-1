// Copyright 2010 Geoffrey 'Phogue' Green
// 
// http://www.phogue.net
//  
// This file is part of PRoCon Frostbite.
//  
// PRoCon Frostbite is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// PRoCon Frostbite is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//  
// You should have received a copy of the GNU General Public License
// along with PRoCon Frostbite.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Specialized;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;

namespace PRoCon.Core.HttpServer
{

    [Serializable]
    public class HttpWebServerRequestData
    {

        public string Method
        {
            get;
            private set;
        }

        public string Request
        {
            get;
            private set;
        }

        public string RequestPath
        {
            get;
            private set;
        }

        public string RequestFile
        {
            get;
            private set;
        }

        public NameValueCollection Query
        {
            get;
            private set;
        }

        public string HttpVersion
        {
            get;
            private set;
        }

        public string Post
        {
            get;
            private set;
        }

        public WebHeaderCollection Headers
        {
            get;
            private set;
        }

        public HttpWebServerRequestData(string document)
        {
            this.Method = String.Empty;
            this.Request = String.Empty;
            this.RequestPath = String.Empty;
            this.RequestFile = String.Empty;
            this.HttpVersion = String.Empty;
            this.Post = String.Empty;
            this.Query = new NameValueCollection();
            this.Headers = new WebHeaderCollection();

            Match methodFileMatch = Regex.Match(document, @"^(?<method>GET|POST) (?<request>.*?) HTTP/(?<http_version>[0-9\.]*)[\r\n]+?", RegexOptions.IgnoreCase);
            if (methodFileMatch.Success == true)
            {

                this.Method = methodFileMatch.Groups["method"].Value;
                this.Request = methodFileMatch.Groups["request"].Value;
                this.HttpVersion = methodFileMatch.Groups["http_version"].Value;

                string[] requestQueryString = this.Request.Split(new char[] { '?' }, 2, StringSplitOptions.RemoveEmptyEntries);

                if (requestQueryString.Length >= 1)
                {
                    int lastIndexForwardSlash = requestQueryString[0].LastIndexOf('/') + 1;

                    this.RequestPath = requestQueryString[0].Substring(0, lastIndexForwardSlash);
                    this.RequestFile = requestQueryString[0].Substring(lastIndexForwardSlash, requestQueryString[0].Length - lastIndexForwardSlash);

                    if (requestQueryString.Length >= 2)
                    {
                        this.Query = HttpUtility.ParseQueryString(requestQueryString[1]);
                    }
                }

                document = document.Remove(0, methodFileMatch.Value.Length);

                this.Headers = new WebHeaderCollection();
                Match headersMatch = Regex.Match(document, @"(?<header>.*?)[ ]*?:[ ]*?(?<value>.*?)[\r\n]+?");

                while (headersMatch.Success == true)
                {

                    this.Headers.Set(headersMatch.Groups["header"].Value, headersMatch.Groups["value"].Value);

                    document = document.Remove(0, headersMatch.Value.Length);

                    headersMatch = headersMatch.NextMatch();
                }

                if (System.String.Compare(this.Method, "POST", System.StringComparison.OrdinalIgnoreCase) == 0)
                {
                    // Read additional post data.
                }
            }
        }
    }

}
