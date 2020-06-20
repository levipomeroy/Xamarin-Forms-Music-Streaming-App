using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Xamarin.Forms;

namespace Android_Music_App.Models
{
    public class PlaylistObject
    {
        public string Id { get; set; }
        public string ImageSource { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string VideoCount { get; set; }

        public PlaylistObject(string id, string imageUrl, string title, string url, string videoCount)
        {
            Id = id;
            ImageSource = imageUrl;
            Title = title;
            Url = url;
            VideoCount = videoCount;
            CleanTitle();
        }
        public PlaylistObject() { }

        public PlaylistObject(PlaylistObject theOG) 
        {
            Id = theOG.Id;
            ImageSource = theOG.ImageSource;
            Title = theOG.Title;
            Url = theOG.Url;
            VideoCount = theOG.VideoCount;
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
            Title = Title.Replace("/U0026", "&");
            //remove multiple spaces
            Title = Regex.Replace(Title, @"\s+", " ");
            Title = Title.Trim();
        }
    }
}
