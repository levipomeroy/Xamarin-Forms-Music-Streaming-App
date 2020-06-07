using Android_Music_App.Models;
using Android_Music_App.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using TagLib.Riff;
using Xamarin.Forms;
using YouTubeSearch;

namespace Android_Music_App
{
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        private ObservableCollection<PlaylistObject> YoutubeSearchResults;
        private ObservableCollection<PlaylistObject> PopularPlaylistResults;
        private ObservableCollection<PlaylistObject> RecentlyPlayedPlaylists;
        private List<SearchResultsObject> _savedSongs;
        private List<SearchResultsObject> _recentlyPlayedPlaylistSongs;

        public MainPage()
        {
            InitializeComponent();
            BindingContext = new PlaylistObject();

            try
            {
                FileManager.InitFolders();
                FileManager.CleanUpMusicFolder();

                InitPlaylists();
            }
            catch (Exception ex)
            {
                FileManager.LogError("Error setting up home page", ex);
            }
        }

        public async void InitPlaylists()
        {
            //Get popular songs playlists
            await GetPopularSongs();

            //Get recently played playlists and count of songs in custom playlist based on recently played
            var playlists = FileManager.GetPlaylists();
            RecentlyPlayedPlaylists = new ObservableCollection<PlaylistObject>(playlists);
            RecentlyPlayedPlaylistsUIObj.ItemsSource = RecentlyPlayedPlaylists;
            var songcount = playlists.Take(4).Sum(x => Convert.ToInt32(Regex.Replace(x.VideoCount, @"[^\d]", "")));
            RecentPlaylistsSongCount.Text = $"{songcount} songs";

            //Get saved songs and update UI with count
            _savedSongs = FileManager.GetSavedSongs();
            SavedSongsPlaylistCount.Text = $"{_savedSongs.Count} songs";

            //Get song info for custom playlist based on recently played 
            await GetSongsForRecentPlaylists(playlists);
        }

        protected override void OnAppearing() //on page load
        {
            base.OnAppearing();

            //Update the recently played playlists
            RecentlyPlayedPlaylists = new ObservableCollection<PlaylistObject>(FileManager.GetPlaylists());
            RecentlyPlayedPlaylistsUIObj.ItemsSource = RecentlyPlayedPlaylists;
        }

        public async Task GetPopularSongs()
        {
            //var playlists = new List<PlaylistSearchComponents>();
            //await Task.Run(async () =>
            //{
                var playlistClient = new PlaylistSearch();
                var playlists = await playlistClient.GetPlaylists("top popular hits", 1);
                playlists.Shuffle();
            //});

            PopularPlaylistResults = new ObservableCollection<PlaylistObject>(playlists.Select(x => new PlaylistObject(x.getId(), x.getThumbnail(), x.getTitle().CleanTitle(), x.getUrl(), $"{x.getVideoCount()} songs")));
            PopularPlaylists.ItemsSource = PopularPlaylistResults;
        }

        public async void SearchButtonPressed_Handler(object sender, System.EventArgs e)
        {
            var playlistClient = new PlaylistSearch();
            var playlists = await playlistClient.GetPlaylists(MusicSearchBar.Text, 1);
            YoutubeSearchResults = new ObservableCollection<PlaylistObject>(playlists.Select(x=> new PlaylistObject(x.getId(), x.getThumbnail(), x.getTitle().CleanTitle(), x.getUrl(), $"{x.getVideoCount()} songs")));

            MusicResults.ItemsSource = YoutubeSearchResults;
            MusicResults.IsVisible = true; //show search results
            MusicDiscovery.IsVisible = false; //hide music discovery
        }

        private async void SongPickedByUser(object sender, SelectedItemChangedEventArgs e)
        {
            var selection = (PlaylistObject)e.SelectedItem;
            FileManager.AddPlaylist(selection);

            await Navigation.PushModalAsync(new NavigationPage(new MediaPlayerPage(selection)));
        }

        private async void SongPickedFromList(object sender, SelectionChangedEventArgs e)
        {
            var selection = e.CurrentSelection.FirstOrDefault() as PlaylistObject;
            FileManager.AddPlaylist(selection);

            await Navigation.PushModalAsync(new NavigationPage(new MediaPlayerPage(selection)));
        }

        private void SearchBarOnTextChanged(object sender, TextChangedEventArgs textChangedEventArgs)
        {
            if (string.IsNullOrWhiteSpace(textChangedEventArgs.NewTextValue)) //new text is empty
            {
                YoutubeSearchResults = null; //reset search results
                MusicResults.ItemsSource = YoutubeSearchResults; //update listView
                MusicResults.IsVisible = false; //hide search results
                MusicDiscovery.IsVisible = true; //show music discovery
            }
        }

        public async void SavedSongsPlayListClicked(object sender, System.EventArgs e)
        {
            _savedSongs.Shuffle();
            await Navigation.PushModalAsync(new NavigationPage(new MediaPlayerPage(null, new Stack<SearchResultsObject>(_savedSongs))));
        }

        public async void RecentlyPlayedPlayListClicked(object sender, System.EventArgs e)
        {
            _recentlyPlayedPlaylistSongs.Shuffle();
            await Navigation.PushModalAsync(new NavigationPage(new MediaPlayerPage(null, new Stack<SearchResultsObject>(_recentlyPlayedPlaylistSongs))));
        }

        public async Task GetSongsForRecentPlaylists(List<PlaylistObject> playlists)
        {
            _recentlyPlayedPlaylistSongs = new List<SearchResultsObject>();

            foreach (var playlist in playlists.Take(4))
            {
                //await Task.Run(async () =>
                //{
                var songsInPlayListClient = new PlaylistItemsSearch();
                var songs = await songsInPlayListClient.GetPlaylistItems(playlist.Url);
                _recentlyPlayedPlaylistSongs.AddRange(songs.Select(x => new SearchResultsObject(x.getTitle(), x.getThumbnail(), GetSongIdFromUrl(x.getUrl()))));
                //});
            }

            //RecentPlaylistsSongCount.Text = $"{_recentlyPlayedPlaylistSongs.Count} songs";
        }

        //helper method
        public string GetSongIdFromUrl(string url)
        {
            var query = HttpUtility.ParseQueryString(new Uri(url).Query);
            return query["v"];
        }

    }
}
