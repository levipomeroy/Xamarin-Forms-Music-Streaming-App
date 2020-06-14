using System;
using Xamarin.Forms;

namespace Android_Music_App.Models
{
    public class SearchResultsObject
    {
        public string Title { get; set; }
        public string ImageSource { get; set; }

        public string Id { get; set; }
        public string Artist { get; set; }

        public SearchResultsObject(string title, string imageUrl, string id, string artist = null)
        {
            Title = title;
            ImageSource = imageUrl;
            Id = id;
            Artist = artist;
        }
        public SearchResultsObject() { }
    }
}
