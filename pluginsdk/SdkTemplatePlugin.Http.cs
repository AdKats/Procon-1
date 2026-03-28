/*
 * SdkTemplatePlugin — HTTP Requests (Partial Class)
 *
 * Shows two approaches:
 *   1. HttpClient (built-in, low-level)
 *   2. Flurl (axios-style, fluent API)
 *
 * Both work. Flurl is cleaner for JSON APIs.
 */

using System;
using System.Net.Http;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Flurl;
using Flurl.Http;

namespace PRoConEvents
{
    public partial class SdkTemplatePlugin
    {
        // =================================================================
        // Option 1: HttpClient — built-in, more verbose
        // =================================================================

        private string HttpClientExample(string playerName)
        {
            try
            {
                var client = new HttpClient();

                // GET request
                string json = client.GetStringAsync(
                    "https://api.example.com/player?name=" + Uri.EscapeDataString(playerName)
                ).Result;

                var data = JObject.Parse(json);
                return data["status"]?.ToString();
            }
            catch (Exception ex)
            {
                Log("Info", "HTTP error: {0}", ex.Message);
                return null;
            }
        }

        // =================================================================
        // Option 2: Flurl — axios-style, fluent API
        // =================================================================

        private string FlurlExample(string playerName)
        {
            try
            {
                // GET with query params
                var result = "https://api.example.com/player"
                    .SetQueryParam("name", playerName)
                    .GetJsonAsync<JObject>()
                    .Result;

                return result["status"]?.ToString();
            }
            catch (Exception ex)
            {
                Log("Info", "HTTP error: {0}", ex.Message);
                return null;
            }
        }

        // =================================================================
        // More Flurl examples
        // =================================================================

        private void FlurlShowcase()
        {
            // GET with headers
            var data = "https://api.example.com/data"
                .WithHeader("Authorization", "Bearer my-token")
                .WithHeader("Accept", "application/json")
                .GetJsonAsync<JObject>()
                .Result;

            // POST JSON body
            var response = "https://api.example.com/report"
                .PostJsonAsync(new
                {
                    player = "PlayerA",
                    reason = "Suspicious activity",
                    timestamp = DateTime.UtcNow
                })
                .Result;

            // POST form data
            var formResponse = "https://api.example.com/login"
                .PostUrlEncodedAsync(new
                {
                    username = "admin",
                    password = "secret"
                })
                .Result;

            // GET with timeout
            var timedResult = "https://api.example.com/slow"
                .WithTimeout(5) // 5 seconds
                .GetStringAsync()
                .Result;

            // Download to string and parse manually
            string raw = "https://api.example.com/text"
                .GetStringAsync()
                .Result;

            // Check response status without throwing
            try
            {
                var resp = "https://api.example.com/check"
                    .GetAsync()
                    .Result;
                int statusCode = resp.StatusCode;
                Log("Debug", "API returned status: {0}", statusCode);
            }
            catch (FlurlHttpException ex)
            {
                Log("Info", "API error: {0} - {1}", ex.StatusCode, ex.Message);
            }
        }
    }
}
