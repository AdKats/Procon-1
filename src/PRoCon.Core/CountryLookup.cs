using MaxMind.GeoIP2;
using MaxMind.GeoIP2.Exceptions;
using PRoCon.Core.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace MaxMind
{
    public class PIP
    {
        public string IP_Address { get; set; }
        public string CountryCode { get; set; }
        public string CountryName { get; set; }
    }

    /// <summary>
    /// Country lookup backed by MaxMind GeoIP2 / GeoLite2 (.mmdb) database.
    /// The public API is kept identical to the legacy implementation so that
    /// all existing callers continue to compile without changes.
    /// </summary>
    public class CountryLookup
    {
        private readonly DatabaseReader _reader;
        private readonly ConcurrentDictionary<string, PIP> _cache = new ConcurrentDictionary<string, PIP>();

        public OptionsSettings OptionsSettings { get; private set; }

        /// <summary>
        /// Creates a new CountryLookup.
        /// <paramref name="fileName"/> is accepted for backward compatibility but ignored.
        /// The constructor probes for a GeoLite2-Country.mmdb file in the application
        /// directory. If the file is missing the instance still works — every lookup
        /// simply returns the "unknown" country ("--" / "N/A").
        /// </summary>
        public CountryLookup(string fileName)
        {
            try
            {
                // Prefer the new .mmdb file next to the application binary.
                string mmdbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GeoLite2-Country.mmdb");
                if (File.Exists(mmdbPath))
                {
                    _reader = new DatabaseReader(mmdbPath);
                }
                else
                {
                    // Also check the legacy location name without "Lite2" prefix.
                    mmdbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GeoIP2-Country.mmdb");
                    if (File.Exists(mmdbPath))
                    {
                        _reader = new DatabaseReader(mmdbPath);
                    }
                }
            }
            catch (Exception)
            {
                // If the database cannot be loaded we degrade gracefully.
                _reader = null;
            }
        }

        // ----------------------------------------------------------------
        // Public API — signatures preserved from the legacy implementation
        // ----------------------------------------------------------------

        public string lookupCountryCode(string str)
        {
            if (!IPAddress.TryParse(str, out IPAddress addr))
                return "--";
            return lookupCountryCode(addr);
        }

        public string lookupCountryCodeGeoIpFile(string str)
        {
            return lookupCountryCode(str);
        }

        public string lookupCountryCode(IPAddress addr)
        {
            return LookupInternal(addr).CountryCode;
        }

        public string lookupCountryCodeGeoIpFile(IPAddress addr)
        {
            return lookupCountryCode(addr);
        }

        public string lookupCountryName(string str)
        {
            if (!IPAddress.TryParse(str, out IPAddress addr))
                return "N/A";
            return lookupCountryName(addr);
        }

        public string lookupCountryNameGeoIpFile(string str)
        {
            return lookupCountryName(str);
        }

        public string lookupCountryName(IPAddress addr)
        {
            return LookupInternal(addr).CountryName;
        }

        public string lookupCountryNameGeoIpFile(IPAddress addr)
        {
            return lookupCountryName(addr);
        }

        public PIP ProxyCheckRequest(IPAddress addr)
        {
            // Legacy method — now returns whatever the local database knows.
            return LookupInternal(addr);
        }

        // ----------------------------------------------------------------
        // Internal helpers
        // ----------------------------------------------------------------

        private PIP LookupInternal(IPAddress addr)
        {
            string key = addr.ToString();
            if (_cache.TryGetValue(key, out PIP cached))
                return cached;

            PIP result;
            if (_reader != null)
            {
                try
                {
                    var response = _reader.Country(addr);
                    result = new PIP
                    {
                        IP_Address = key,
                        CountryCode = response.Country.IsoCode ?? "--",
                        CountryName = response.Country.Name ?? "N/A"
                    };
                }
                catch (AddressNotFoundException)
                {
                    result = UnknownPip(key);
                }
                catch (GeoIP2Exception)
                {
                    result = UnknownPip(key);
                }
                catch (Exception)
                {
                    result = UnknownPip(key);
                }
            }
            else
            {
                result = UnknownPip(key);
            }

            _cache[key] = result;
            return result;
        }

        private static PIP UnknownPip(string ip)
        {
            return new PIP
            {
                IP_Address = ip,
                CountryCode = "--",
                CountryName = "N/A"
            };
        }
    }
}
