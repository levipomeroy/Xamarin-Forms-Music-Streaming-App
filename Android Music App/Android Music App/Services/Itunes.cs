using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Android_Music_App.Services
{
    public static class Itunes
    {
        public static readonly string BASE_URL = "https://itunes.apple.com/search?term=";
        public static dynamic GetDataFromItunes(string searchFor, int limit)
        {
            try
            {
                searchFor = searchFor.ToLower().Replace("video", "").Replace("lyrics", "").Replace("official", "").Replace("music", "");

                var result = string.Empty;
                var url = $"{BASE_URL}{searchFor}&limit={limit}";

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.ContentType = "application/json; charset=UTF-8";
                //request.AutomaticDecompression = DecompressionMethods.GZip;

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    result = reader.ReadToEnd();
                }

                var jsonObj = JObject.Parse(result);
                if (Convert.ToInt32(jsonObj.GetValue("resultCount")) == 0)
                {
                    //FileManager.LogInfo($"Failed to get info from Itunes, url: {url}");
                    return null;
                }

                var jsonObjResults = JObject.Parse(result).GetValue("results")[0] as JObject;
                var fullResult = new
                {
                    ArtistName = jsonObjResults.GetValue("artistName"),
                    CollectionName = jsonObjResults.GetValue("collectionName"),
                    TrackName = jsonObjResults.GetValue("trackName"),
                    ArtworkUrl100 = jsonObjResults.GetValue("artworkUrl100").ToString().Replace("100x100bb", "400x400bb")
                };

                return fullResult;
            }
            catch (Exception ex)
            {
                FileManager.LogError("Error getting info from itunes", ex);
            }

            return null;
        }
    }
}
