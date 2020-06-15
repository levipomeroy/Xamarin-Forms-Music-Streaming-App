using Android_Music_App.Models;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;

namespace Android_Music_App.Services
{
    public static class Itunes
    {
        public static readonly string BASE_URL = "https://itunes.apple.com/search?term=";
        public static SearchResultsObject GetDataFromItunes(SearchResultsObject song, int limit)
        {
            try
            {
                song.Artist = song.Title.GetArtistName();
                song.Title = song.Title.CleanTitle();

                var searchFor = song.Title.CleanTitle().ToLower()
                    .Replace("official", "")
                    .Replace("music", "")
                    .Replace("video", "")
                    .Replace("lyrics", "");

                if(searchFor.Contains("ft."))
                {
                    searchFor = searchFor.Substring(0, song.Title.ToLower().IndexOf("ft."));
                }

                var result = string.Empty;
                var url = $"{BASE_URL}{searchFor}&limit={limit}";

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.ContentType = "application/json; charset=UTF-8";

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    result = reader.ReadToEnd();
                }

                if (string.IsNullOrEmpty(result)) { return song; }

                var jsonObj = JObject.Parse(result);
                if (jsonObj == null || Convert.ToInt32(jsonObj.GetValue("resultCount")) == 0)
                {
                    FileManager.LogInfo($"Failed to get info from Itunes, url: {url}");
                    return song;
                }

                var jsonObjResults = JObject.Parse(result).GetValue("results")[0] as JObject;
                return new SearchResultsObject
                {
                    Artist = Convert.ToString(jsonObjResults.GetValue("artistName")),
                   // CollectionName = jsonObjResults.GetValue("collectionName"),
                    Title = Convert.ToString(jsonObjResults.GetValue("trackName")),
                    ImageSource = jsonObjResults.GetValue("artworkUrl100").ToString().Replace("100x100bb", "400x400bb")
                };
            }
            catch (Exception ex)
            {
                FileManager.LogError("Error getting info from itunes", ex);
                return song;
            }
        }
    }
}
