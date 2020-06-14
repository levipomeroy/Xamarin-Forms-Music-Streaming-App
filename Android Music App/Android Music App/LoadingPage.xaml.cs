using Android_Music_App.Models;
using Android_Music_App.Services;
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
            //Store that this playlist was selected
            FileManager.AddPlaylist(selectedPlaylist);

            //Get songs in playlist
            var songsInPlayListClient = new PlaylistItemsSearch();
            var songs = await songsInPlayListClient.GetPlaylistItems(selectedPlaylist.Url);
            songs.Shuffle();
            var chosenPlaylist = new Stack<SearchResultsObject>(songs.Select(x => new SearchResultsObject(x.getTitle(), x.getThumbnail(), GetSongIdFromUrl(x.getUrl()))));

            //Download first song in playlist
            var firstSong = chosenPlaylist.FirstOrDefault();
            await FileManager.DownloadSingleSong(firstSong);

            var trackInfo = Itunes.GetDataFromItunes(firstSong.Title.CleanTitle(), 1);
            chosenPlaylist.FirstOrDefault().Title = trackInfo.TrackName ?? firstSong.Title;
            chosenPlaylist.FirstOrDefault().ImageSource = trackInfo.ArtworkUrl100 ?? firstSong.ImageSource;
            chosenPlaylist.FirstOrDefault().Artist = trackInfo.ArtistName ?? firstSong.Title.GetArtistName();

            //load media player page 
            await Navigation.PushModalAsync(new NavigationPage(new MediaPlayerPage(chosenPlaylist)));
        }

        public async void LoadRecentlyPlayedPlaylists()
        {
            var recentlyPlayedPlaylistSongs = new List<SearchResultsObject>();
            var songsInPlayListClient = new PlaylistItemsSearch();

            foreach (var playlist in FileManager.GetPlaylists().Take(4))
            {
                var songs = await songsInPlayListClient.GetPlaylistItems(playlist.Url);
                recentlyPlayedPlaylistSongs.AddRange(songs.Select(x => new SearchResultsObject(x.getTitle(), x.getThumbnail(), GetSongIdFromUrl(x.getUrl()))));
            }

            recentlyPlayedPlaylistSongs.Shuffle();

            //Download first song in playlist
            var firstSong = recentlyPlayedPlaylistSongs.FirstOrDefault();
            await FileManager.DownloadSingleSong(firstSong);

            var trackInfo = Itunes.GetDataFromItunes(firstSong.Title.CleanTitle(), 1);
            recentlyPlayedPlaylistSongs.FirstOrDefault().Title = trackInfo.TrackName ?? firstSong.Title;
            recentlyPlayedPlaylistSongs.FirstOrDefault().ImageSource = trackInfo.ArtworkUrl100 ?? firstSong.ImageSource;
            recentlyPlayedPlaylistSongs.FirstOrDefault().Artist = trackInfo.ArtistName ?? firstSong.Title.GetArtistName();

            await Navigation.PushModalAsync(new NavigationPage(new MediaPlayerPage(new Stack<SearchResultsObject>(recentlyPlayedPlaylistSongs))));
        }

        public async void LoadSavedSongs()
        {
            var savedSongs = FileManager.GetSavedSongs();
            savedSongs.Shuffle();

            //Download first song in playlist
            var firstSong = savedSongs.FirstOrDefault();
            await FileManager.DownloadSingleSong(firstSong);

            var trackInfo = Itunes.GetDataFromItunes(firstSong.Title.CleanTitle(), 1);
            savedSongs.FirstOrDefault().Title = trackInfo.TrackName ?? firstSong.Title;
            savedSongs.FirstOrDefault().ImageSource = trackInfo.ArtworkUrl100 ?? firstSong.ImageSource;
            savedSongs.FirstOrDefault().Artist = trackInfo.ArtistName ?? firstSong.Title.GetArtistName();

            await Navigation.PushModalAsync(new NavigationPage(new MediaPlayerPage(new Stack<SearchResultsObject>(savedSongs))));
        }

        //helper method
        public string GetSongIdFromUrl(string url)
        {
            var query = HttpUtility.ParseQueryString(new Uri(url).Query);
            return query["v"];
        }

    }
}