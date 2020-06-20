using Android_Music_App.Models;
using Android_Music_App.Services;
using AngleSharp.Dom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using YouTubeSearch;

namespace Android_Music_App
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoadingPage : ContentPage
    {
        public LoadingPage(PlaylistObject selectedPlaylist = null, bool recentlyPlayedPlaylist = false, bool savedSongs = false)
        {
            InitializeComponent();

            if (recentlyPlayedPlaylist)
            {
                LoadRecentlyPlayedPlaylists();
            }
            else if (savedSongs)
            {
                LoadSavedSongs();
            }
            else
            {
                LoadPlaylist(selectedPlaylist);
            }
        }

        public async void LoadPlaylist(PlaylistObject selectedPlaylist)
        {
            AddNewLoadingStatus("Getting playlist info");

            //Store that this playlist was selected
            FileManager.AddPlaylist(selectedPlaylist);

            //Get songs in playlist
            PreviousStepComplete();
            AddNewLoadingStatus("Gathering songs in playlist");

            var songsInPlayListClient = new PlaylistItemsSearch();
            var songs = await songsInPlayListClient.GetPlaylistItems(selectedPlaylist.Url);
            PreviousStepComplete();
            AddNewLoadingStatus("Shuffling");
            songs.Shuffle();

            var chosenPlaylist = new List<Song>(songs.Select(x => new Song(x.getTitle(), x.getThumbnail(), GetSongIdFromUrl(x.getUrl()))));

            //Download first song in playlist
            //PreviousStepComplete();
            //AddNewLoadingStatus("Downloading");

            var firstSong = chosenPlaylist.FirstOrDefault();
            //await FileManager.DownloadSingleSong(firstSong);

            PreviousStepComplete();
            AddNewLoadingStatus("Retrieving album art");
            chosenPlaylist.FirstOrDefault().GetDataFromItunes(); 

            PreviousStepComplete();
            AddNewLoadingStatus("Playing");

            //load media player page 
            await Navigation.PushModalAsync(new NavigationPage(new MediaPlayerPage(chosenPlaylist)));
        }

        public async void LoadRecentlyPlayedPlaylists()
        {
            AddNewLoadingStatus("Getting playlist info");

            var recentlyPlayedPlaylistSongs = new List<Song>();
            var songsInPlayListClient = new PlaylistItemsSearch();

            PreviousStepComplete();
            AddNewLoadingStatus("Gathering songs in playlist");

            foreach (var playlist in FileManager.GetPlaylists().Take(4))
            {
                var songs = await songsInPlayListClient.GetPlaylistItems(playlist.Url);
                recentlyPlayedPlaylistSongs.AddRange(songs.Select(x => new Song(x.getTitle(), x.getThumbnail(), GetSongIdFromUrl(x.getUrl()))));
            }

            PreviousStepComplete();
            AddNewLoadingStatus("Shuffling");
            recentlyPlayedPlaylistSongs.Shuffle();

            //Download first song in playlist
            PreviousStepComplete();
            AddNewLoadingStatus("Getting stream");
            var firstSong = recentlyPlayedPlaylistSongs.FirstOrDefault();

            //await FileManager.DownloadSingleSong(firstSong);

            PreviousStepComplete();
            AddNewLoadingStatus("Retrieving album art");
            recentlyPlayedPlaylistSongs.FirstOrDefault().GetDataFromItunes();

            PreviousStepComplete();
            AddNewLoadingStatus("Playing");
            await Navigation.PushModalAsync(new NavigationPage(new MediaPlayerPage(new List<Song>(recentlyPlayedPlaylistSongs))));
        }

        public async void LoadSavedSongs()
        {
            AddNewLoadingStatus("Getting playlist info");

            PreviousStepComplete();
            AddNewLoadingStatus("Gathering songs in playlist");
            var savedSongs = FileManager.GetSavedSongs();

            PreviousStepComplete();
            AddNewLoadingStatus("Shuffling");
            savedSongs.Shuffle();

            //Download first song in playlist
            var firstSong = savedSongs.FirstOrDefault();
            //await FileManager.DownloadSingleSong(firstSong);

            PreviousStepComplete();
            AddNewLoadingStatus("Retrieving album art");
            savedSongs.FirstOrDefault().GetDataFromItunes();


            PreviousStepComplete();
            AddNewLoadingStatus("Playing");
            await Navigation.PushModalAsync(new NavigationPage(new MediaPlayerPage(new List<Song>(savedSongs))));
        }

        //helper method
        public string GetSongIdFromUrl(string url)
        {
            var query = HttpUtility.ParseQueryString(new Uri(url).Query);
            return query["v"];
        }

        public void AddNewLoadingStatus(string status)
        {
            mainLoadingLayout.Children.Add(new Label
            {
                Text = status,
                HorizontalOptions = LayoutOptions.Center,
                FontSize = 20,
                Style = Application.Current.Resources["MaterialIcons"] as Style
            });
        }

        public void PreviousStepComplete()
        {
            (mainLoadingLayout.Children.LastOrDefault() as Label).Text += "\t\U000f05e0";
        }

    }
}