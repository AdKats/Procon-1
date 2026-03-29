using System;
using System.Net;

namespace BF4DB_API
{
    class BF4DB_API
    {


        public static String postAPI(String uri, String parameters)
        {
            using (var bf4db_client = new WebClient())
            {
                bf4db_client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                String url = "https://bf4db.com/api/procon/";
                String html_data = bf4db_client.UploadString(url + uri, parameters);
                return html_data;
            }
        }

        public String verifyKey(String apiKey, String pluginVersion, string serverVersion)
        {
            try
            {
                String uri = "verify/key";
                String parameters = "api_key=" + apiKey + "&plugin_version=" + pluginVersion + "&server_version=" + serverVersion;
                return postAPI(uri, parameters);
            }
            catch (WebException ex)
            {
                throw new Exception(ex.Status + "|verifyKey|" + pluginVersion + "|" + serverVersion);
            }
            catch
            {
                throw new Exception("An error occured while trying to contact BF4DB");
            }
        }

        public String updateServer(String apiKey)
        {
            try
            {
                String uri = "update/server";
                String parameters = "api_key=" + apiKey;
                return postAPI(uri, parameters);
            }
            catch (WebException ex)
            {
                throw new Exception(ex.Status + "|updateServer");
            }
            catch
            {
                throw new Exception("An error occured while updating server");
            }
        }

        public static string kickPlayer(String playername, String apiKey)
        {
            try
            {
                string uri = "kick/" + playername;
                string parameters = "api_key=" + apiKey;
                return postAPI(uri, parameters);
            }
            catch (WebException ex)
            {
                throw new Exception(ex.Status + "|kickPlayer|" + playername + "|");
            }
            catch
            {
                throw new Exception("An error occured while kicking player");
            }
        }

        public string violationWeapon(String playername, String weapon, String apiKey)
        {
            try
            {
                string uri = "weaponviolation/" + playername;
                string parameters = "weapon=" + weapon + "&api_key=" + apiKey;
                return postAPI(uri, parameters);
            }
            catch (WebException ex)
            {
                throw new Exception(ex.Status + "|violationWeapon|" + playername + "|" + weapon);
            }
            catch (Exception)
            {
                throw new Exception("An error occured while sending weapon violation");
            }
        }

        public String checkPlayer(String playername, String guid, String apiKey)
        {
            try
            {
                String uri = "check/player/" + playername;
                String parameters = "guid=" + guid + "&api_key=" + apiKey;
                return postAPI(uri, parameters);
            }
            catch (WebException ex)
            {
                throw new Exception(ex.Status + "|checkPlayer|" + playername + "|" + guid);
            }
            catch
            {
                throw new Exception("An error occured while checking player");
            }
        }

        public String violationPlayer(String playername, String encodedMsg, String apiKey)
        {
            try
            {
                String uri = "violation/" + playername;
                String parameters = "violation=" + encodedMsg + "&api_key=" + apiKey;
                return postAPI(uri, parameters);
            }
            catch (WebException ex)
            {
                throw new Exception(ex.Status + "|violationPlayer|" + playername + "|" + encodedMsg);
            }
            catch
            {
                throw new Exception("An error occured while sending player violation");
            }
        }

        public String updatePB(String SoldierName, String GUID, String Ip, String PlayerCountryCode, String apiKey)
        {
            try
            {
                String uri = "update/pb/" + SoldierName;
                String parameters = "pb_guid=" + GUID + "&pb_ip=" + Ip + "&pb_country=" + PlayerCountryCode + "&api_key=" + apiKey;
                return postAPI(uri, parameters);
            }
            catch (WebException ex)
            {
                throw new Exception(ex.Status + "|updatePB|" + SoldierName + "|" + GUID + "|" + Ip + "|" + PlayerCountryCode);
            }
            catch
            {
                throw new Exception("An error occured while updating PB on soldier " + SoldierName);
            }
        }
    }
}