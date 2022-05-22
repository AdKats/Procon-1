using Newtonsoft.Json;
using PRoCon.Core.Remote;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRoCon.Core.ProxyChecker
{
    public static class ProconProxyChecker
    {
        public static Hashtable IPDB = new Hashtable();

        // Create a async task to make a web request to the API. Pass an IP address to the API.
        // The API URL is: https://api.myrcon.net/proxycheck/
        // Append the IP address to the end of the URL.
        // The API will return a JSON response.
        // The JSON response will look like:
        // {
        //    "status": "ok",
        //    "node": "CRONUS",
        //    "1.1.1.1": {
        //      "asn": "AS13335",
        //      "provider": "Cloudflare, Inc.",
        //      "continent": "Oceania",
        //      "country": "Australia",
        //      "isocode": "AU",
        //      "latitude": -33.494,
        //      "longitude": 143.2104,
        //      "proxy": "no",
        //      "type": "Business",
        //      "risk": 0
        //    }
        //    "block": "no",
        //    "block_reason": "na"
        // }
        public static bool CheckProxy(string ipAddress, PRoConClient client, CPunkbusterInfo pbinfo)
        {
            string response = "";
            
            if (ipAddress == "")
            {
                return false;
            }

            // Check if ipAddress already exists in IPDB
            if (IPDB.ContainsKey(ipAddress))
            {
                return true;
            }

            try
            {
                // Create a new web request to the API using Task.Run
                Task<bool>.Run(async () =>
                {
                    using (var webClient = new System.Net.WebClient())
                    {
                        response = await webClient.DownloadStringTaskAsync("https://api.myrcon.net/proxycheck/" + ipAddress);
                    }
                }).Wait();


                var t = Task<bool>.Run(async () =>
                {
                    using (var client = new System.Net.Http.HttpClient())
                    {
                        // Set the API URL.
                        string url = "https://api.myrcon.net/proxycheck/";

                        // Append the IP address to the end of the URL.
                        url += ipAddress;

                        // Make the web request.
                        response = await client.GetStringAsync(url);

                        Hashtable ht = JsonConvert.DeserializeObject<Hashtable>(response);

                        // Store the JSON response in a hash table using the IP address as the key.
                        // The hash table will be used to store the JSON response.
                        IPDB.Add(ipAddress.ToString(), ht);
                    }
                    
                    return true;
                });

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Get a record from the IPDB or return null when empty.
        public static Hashtable? GetRecord(string ipAddress)
        {
            if (HasRecord(ipAddress))
            {
                return (Hashtable)IPDB[ipAddress];
            }
            else
            {
                return null;
            }
        }

        public static Boolean HasRecord(string ipAddress)
        {
            return IPDB.ContainsKey(ipAddress);
        }

        public static string CountryCode(string ip)
        {
            Hashtable a = (Hashtable)IPDB[ip];
            return (string)a["isocode"];
        }
    }
}
