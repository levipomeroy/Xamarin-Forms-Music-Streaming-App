using Android_Music_App.Services;
using Java.Security;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xamarin.Forms;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace Android_Music_App.Models
{
    public class Song
    {
        private readonly string BASE_URL = "https://itunes.apple.com/search?term=";
        private readonly int LIMIT = 1;

        public string Title { get; set; }
        public string ImageSource { get; set; }
        public string Id { get; set; }
        public string Artist { get; set; }
        public string StreamUrl { get; set; }

        public Song(string title, string imageUrl, string id, string artist = null, string streamUrl = null)
        {
            Title = title;
            ImageSource = imageUrl;
            Id = id;
            Artist = artist;
            StreamUrl = streamUrl;
        }
        public Song() { }

        public async Task GetStream()
        {
            try
            {
                var youtube = new YoutubeClient();
                var streamManifest = await youtube.Videos.Streams.GetManifestAsync(Id).ConfigureAwait(false);
                var streamInfo = streamManifest.GetAudioOnly().WithHighestBitrate(); // get highest bitrate audio-only stream
                //var streamInfo = streamManifest.GetAudioOnly().OrderBy(x => x.Size).FirstOrDefault(); // get smallest audio file

                StreamUrl = streamInfo.Url;
            }
            catch (Exception ex)
            {
                FileManager.LogError($"Error getting stream for song - Title: {Title}, Id: {Id}", ex);
            }
        }

        public void GetDataFromItunes()
        {
            try
            {
                var searchFor = Title.ToLower()
                    .Replace("official", "")
                    .Replace("music", "")
                    .Replace("video", "")
                    .Replace("on screen", "")
                    .Replace("lyrics", "");

                CleanTitle();
                GetArtistName();

                if (searchFor.Contains("ft."))
                {
                    searchFor = searchFor.Substring(0, Title.ToLower().IndexOf("ft."));
                }

                if (!string.IsNullOrWhiteSpace(Artist))
                {
                    searchFor = $"{Artist} {searchFor}";
                }

                var result = string.Empty;
                var url = $"{BASE_URL}{searchFor}&limit={LIMIT}";

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.ContentType = "application/json; charset=UTF-8";

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    result = reader.ReadToEnd();
                }

                if (!string.IsNullOrEmpty(result))
                {
                    var jsonObj = JObject.Parse(result);
                    if (jsonObj != null && Convert.ToInt32(jsonObj.GetValue("resultCount")) > 0)
                    {
                        var jsonObjResults = JObject.Parse(result).GetValue("results")[0] as JObject;

                        Artist = Convert.ToString(jsonObjResults.GetValue("artistName"));
                        Title = Convert.ToString(jsonObjResults.GetValue("trackName"));
                        ImageSource = jsonObjResults.GetValue("artworkUrl100").ToString().Replace("100x100bb", "400x400bb");
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(Artist))
                        {
                            Title = Title.ToLower()
                                .Replace($"{Artist.ToLower()} -", string.Empty)
                                .Replace($"{Artist.ToLower()}-", string.Empty)
                                .Replace($"{Artist.ToLower()}", string.Empty);
                        }

                        Title = Title.ToLower().Replace("official", string.Empty)
                            .Replace("music", string.Empty)
                            .Replace("video", string.Empty)
                            .Replace("full hd", string.Empty)
                            .Replace("on screen", string.Empty)
                            .Replace("lyrics", string.Empty);

                        CleanTitle();

                        FileManager.LogInfo($"Failed to get info from Itunes, url: {url}");
                    }
                }
            }
            catch (Exception ex)
            {
                FileManager.LogError("Error getting info from itunes", ex);
            }
        }

        public void CleanTitle()
        {
            //Remove parathesis 
            Title = Regex.Replace(Title, @"\([^()]*\)", string.Empty);
            //Remove brackets
            Title = Regex.Replace(Title, @"\[.*?\]", string.Empty);
            //Convert to title case 
            Title = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Title.ToLower());
            //remove emojis and junk 
            Title = Regex.Replace(Title, @"[^\u0000-\u007F]+", "");
            //standardize seperator 
            Title = Regex.Replace(Title, "[+|:]", " - ");
            //show ampersand correctly
            Title = Title.Replace("\\U0026", "&");
            //remove multiple spaces
            Title = Regex.Replace(Title, @"\s+", " ");
            Title = Title.Trim();
            if (Title.StartsWith("-"))
            {
                Title = Title.Substring(Title.IndexOf('-') + 1);
            }
            Title = Title.Trim();
        }

        public void GetArtistName()
        {
            Artist = Title.Contains("-") ? Title.Substring(0, Title.IndexOf('-')).Replace("\\U0026", "&") : string.Empty;
        }
    }
}
