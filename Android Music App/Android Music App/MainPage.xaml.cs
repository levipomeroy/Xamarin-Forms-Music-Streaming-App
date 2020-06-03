using Android_Music_App.Models;
using Android_Music_App.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
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

        public MainPage()
        {
            InitializeComponent();
            try
            {
                SongFileManager.InitFolder();
                SongFileManager.CleanUpMusicFolder();
                // Task.Run(() => GetPopularSongs());
                GetPopularSongs();
                //GetEminemSongs();

                BindingContext = new PlaylistObject();
            }
            catch(Exception ex)
            {
                Logger.Error("Error setting up home page", ex);
            }
        }

        public async Task GetPopularSongs()
        {
            await Task.Run(async () =>
            {
                var playlistClient = new PlaylistSearch();
                var playlists = await playlistClient.GetPlaylists("top popular hits", 1);
                PopularPlaylistResults = new ObservableCollection<PlaylistObject>(playlists.Select(x => new PlaylistObject(x.getId(), x.getThumbnail(), x.getTitle(), x.getUrl(), $"{x.getVideoCount()} songs")));
            });

            PopularPlaylists.ItemsSource = PopularPlaylistResults;

            RecentlyPlayedPlaylists = new ObservableCollection<PlaylistObject>(RecentlyPlayed.GetPlaylists());

            EminemPlaylists.ItemsSource = RecentlyPlayedPlaylists;
        }

        public async void SearchButtonPressed_Handler(object sender, System.EventArgs e)
        {
            var playlistClient = new PlaylistSearch();
            var playlists = await playlistClient.GetPlaylists(MusicSearchBar.Text, 1);
            YoutubeSearchResults = new ObservableCollection<PlaylistObject>(playlists.Select(x=> new PlaylistObject(x.getId(), x.getThumbnail(), x.getTitle(), x.getUrl(), $"{x.getVideoCount()} songs")));

            MusicResults.ItemsSource = YoutubeSearchResults;
        }

        private async void SongPickedByUser(object sender, SelectedItemChangedEventArgs e)
        {
            var selection = (PlaylistObject)e.SelectedItem;
            RecentlyPlayed.AddPlaylist(selection);

            await Navigation.PushModalAsync(new NavigationPage(new MediaPlayerPage(selection)));
        }

        private async void SongPickedFromList(object sender, SelectionChangedEventArgs e)
        {
            var selection = e.CurrentSelection.FirstOrDefault() as PlaylistObject;
            RecentlyPlayed.AddPlaylist(selection);

            await Navigation.PushModalAsync(new NavigationPage(new MediaPlayerPage(selection)));
        }
    }
}
