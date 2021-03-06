﻿using Android_Music_App.Models;
using Android_Music_App.Services;
using AngleSharp.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
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
        private List<Song> _savedSongs;
        private PlaylistSearch _playlistSearchClient;
        private PlaylistItemsSearch _playlistitemsClient;

        public MainPage()
        {
            InitializeComponent();
            _playlistSearchClient = new PlaylistSearch();
            _playlistitemsClient = new PlaylistItemsSearch();

            try
            {
                FileManager.InitFolders();

                InitPlaylists();
            }
            catch (Exception ex)
            {
                FileManager.LogError("Error setting up home page", ex);
            }

            BindingContext = new PlaylistObject();
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

            _savedSongs.Shuffle();
            savedSongListImage00.Source = _savedSongs.Count >= 1 ? _savedSongs.GetItemByIndex(0).ImageSource : "https://images.macrumors.com/t/MKlRm9rIBpfcGnjTpf6ZxgpFTUg=/1600x1200/smart/article-new/2018/05/apple-music-note.jpg";
            savedSongListImage01.Source = _savedSongs.Count >= 2 ? _savedSongs.GetItemByIndex(1).ImageSource : "https://images.macrumors.com/t/MKlRm9rIBpfcGnjTpf6ZxgpFTUg=/1600x1200/smart/article-new/2018/05/apple-music-note.jpg";
            savedSongListImage10.Source = _savedSongs.Count >= 3 ? _savedSongs.GetItemByIndex(2).ImageSource : "https://images.macrumors.com/t/MKlRm9rIBpfcGnjTpf6ZxgpFTUg=/1600x1200/smart/article-new/2018/05/apple-music-note.jpg";
            savedSongListImage11.Source = _savedSongs.Count >= 4 ? _savedSongs.GetItemByIndex(3).ImageSource : "https://images.macrumors.com/t/MKlRm9rIBpfcGnjTpf6ZxgpFTUg=/1600x1200/smart/article-new/2018/05/apple-music-note.jpg";

            var playlistCopy = RecentlyPlayedPlaylists.ToList();
            playlistCopy.Shuffle();
            recentPlaylistsPlaylistImage00.Source = playlistCopy.Count >= 1 ? playlistCopy.GetItemByIndex(0).ImageSource : "https://images.macrumors.com/t/MKlRm9rIBpfcGnjTpf6ZxgpFTUg=/1600x1200/smart/article-new/2018/05/apple-music-note.jpg";
            recentPlaylistsPlaylistImage01.Source = playlistCopy.Count >= 2 ? playlistCopy.GetItemByIndex(1).ImageSource : "https://images.macrumors.com/t/MKlRm9rIBpfcGnjTpf6ZxgpFTUg=/1600x1200/smart/article-new/2018/05/apple-music-note.jpg";
            recentPlaylistsPlaylistImage10.Source = playlistCopy.Count >= 3 ? playlistCopy.GetItemByIndex(2).ImageSource : "https://images.macrumors.com/t/MKlRm9rIBpfcGnjTpf6ZxgpFTUg=/1600x1200/smart/article-new/2018/05/apple-music-note.jpg";
            recentPlaylistsPlaylistImage11.Source = playlistCopy.Count >= 4 ? playlistCopy.GetItemByIndex(3).ImageSource : "https://images.macrumors.com/t/MKlRm9rIBpfcGnjTpf6ZxgpFTUg=/1600x1200/smart/article-new/2018/05/apple-music-note.jpg";

            //Tap gesture handler for custom playlist
            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += (s, e) => {
                Navigation.PushModalAsync(new NavigationPage(new LoadingPage(recentlyPlayedPlaylist: true)));
            };
            customPlaylistsButton.GestureRecognizers.Add(tapGestureRecognizer);

            //Tap gesture handler for saved song playlist
            var tapGestureRecognizer2 = new TapGestureRecognizer();
            tapGestureRecognizer2.Tapped += (s, e) => {
                Navigation.PushModalAsync(new NavigationPage(new LoadingPage(savedSongs: true)));
            };
            savedSongsPlayListButton.GestureRecognizers.Add(tapGestureRecognizer2);
        }

        protected override void OnAppearing() //on page load
        {
            base.OnAppearing();

            //Update the recently played playlists
            RecentlyPlayedPlaylists = new ObservableCollection<PlaylistObject>(FileManager.GetPlaylists());
            RecentlyPlayedPlaylistsUIObj.ItemsSource = RecentlyPlayedPlaylists;

            //update saved songs 
            _savedSongs = FileManager.GetSavedSongs();
        }

        public async Task GetPopularSongs()
        {
            var playlists = await _playlistSearchClient.GetPlaylists("top popular hits", 1);
            playlists.Shuffle();

            PopularPlaylistResults = new ObservableCollection<PlaylistObject>(playlists.Select(x => new PlaylistObject(x.getId(), x.getThumbnail(), x.getTitle(), x.getUrl(), $"{x.getVideoCount()} songs")));
            PopularPlaylists.ItemsSource = PopularPlaylistResults;
        }

        public async void SearchButtonPressed_Handler(object sender, System.EventArgs e)
        {
            //var playlistClient = new PlaylistSearch();
            var playlists = await _playlistSearchClient.GetPlaylists(MusicSearchBar.Text, 1);
            YoutubeSearchResults = new ObservableCollection<PlaylistObject>(playlists.Select(x=> new PlaylistObject(x.getId(), x.getThumbnail(), x.getTitle(), x.getUrl(), $"{x.getVideoCount()} songs")));

            MusicResults.ItemsSource = YoutubeSearchResults;
            MusicResults.IsVisible = true; //show search results
            MusicDiscoveryScrollViewWrapper.IsVisible = false; //hide music discovery
        }

        //playlist selected
        private async void PlaylistPickedFromSearchResults(object sender, SelectedItemChangedEventArgs e)
        {
            var selection = e.SelectedItem as PlaylistObject;

            await Navigation.PushModalAsync(new NavigationPage(new LoadingPage(selection)));
        }

        private async void PlaylistPickedFromList(object sender, SelectionChangedEventArgs e)
        {
            var selection = e.CurrentSelection.FirstOrDefault() as PlaylistObject;

            await Navigation.PushModalAsync(new NavigationPage(new LoadingPage(selection)));
        }

        private void SearchBarOnTextChanged(object sender, TextChangedEventArgs textChangedEventArgs)
        {
            if (string.IsNullOrWhiteSpace(textChangedEventArgs.NewTextValue)) //new text is empty
            {
                YoutubeSearchResults = null; //reset search results
                MusicResults.ItemsSource = YoutubeSearchResults; //update listView
                MusicResults.IsVisible = false; //hide search results
                MusicDiscoveryScrollViewWrapper.IsVisible = true; //show music discovery
            }
        }

        //helper method
        public string GetSongIdFromUrl(string url)
        {
            var query = HttpUtility.ParseQueryString(new Uri(url).Query);
            return query["v"];
        }
    }
}
