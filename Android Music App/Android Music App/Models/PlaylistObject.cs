using System;
using Xamarin.Forms;

namespace Android_Music_App.Models
{
    public class PlaylistObject
    {
        public string Id { get; set; }
        //public ImageSource ImageSource { get; set; }
        public string ImageSource { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string VideoCount { get; set; }

        public PlaylistObject(string id, string imageUrl, string title, string url, string videoCount)
        {
            Id = id;
            //ImageSource = ImageSource.FromUri(new Uri(imageUrl));
            ImageSource = imageUrl;
            Title = title;
            Url = url;
            VideoCount = videoCount;
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
    }
}
