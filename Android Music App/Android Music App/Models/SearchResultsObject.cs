using System;
using Xamarin.Forms;

namespace Android_Music_App.Models
{
    public class SearchResultsObject
    {
        public string Title { get; set; }
        public string ImageSource { get; set; }

        public string Id { get; set; }

        public SearchResultsObject(string title, string imageUrl, string id)
        {
            Title = title;
            ImageSource = imageUrl;
            Id = id;
        }
        public SearchResultsObject() { }
    }
}
