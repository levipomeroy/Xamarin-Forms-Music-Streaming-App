﻿using Android.Media;
using Android_Music_App.Models;
using Android_Music_App.Services;
using AngleSharp.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Web;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;
using YouTubeSearch;

namespace Android_Music_App
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MediaPlayerPage : ContentPage
    {
        PlaylistObject _selectedPlayList;
        SearchResultsObject _selectedItem;
        Stack<SearchResultsObject> _songsInPlayList;
        MediaPlayer _mediaPlayer;
        Timer _timer;
        int _tickCount;
        int _downloadedCount;
        int _playedCount;
        private const string FILE_DIR = "/storage/emulated/0/Download/";

        public MediaPlayerPage(PlaylistObject selectedItem)
        {
            _selectedPlayList = selectedItem;
            _mediaPlayer = _mediaPlayer ?? new MediaPlayer();
            _songsInPlayList = new Stack<SearchResultsObject>();

            _tickCount = 0;
            _timer = new Timer();
            _timer.Interval = 1000; // 1 second
            _timer.Elapsed += TimeElapsedHandler;

            InitializeComponent();
            InitPlaylist();

            _mediaPlayer.Looping = false;
            _mediaPlayer.Completion += OnCompleteHandler;
        }


        //Set up
        public async void InitPlaylist()
        {
            await GetSongsInPlaylist(_selectedPlayList.Url);
            await GetFirstSong();
            await GetNextSectionOfSongsInPlaylist();
        }

        private async Task GetSongsInPlaylist(string playListUrl)
        {
            var songsInPlayListClient = new PlaylistItemsSearch();
            var songs = await songsInPlayListClient.GetPlaylistItems(playListUrl);
            _songsInPlayList = new Stack<SearchResultsObject>(songs.Select(x => new SearchResultsObject(x.getTitle(), x.getThumbnail(), GetSongIdFromUrl(x.getUrl()))));
        }

        private async Task GetFirstSong()
        {
            _selectedItem = _songsInPlayList.FirstOrDefault();
            await Downloader.DownloadSingleSong(_selectedItem);
            _downloadedCount++;
            await PlaySelectedSong();

            _songsInPlayList.Pop(); //already set to play
        }


        //continue to download songs
        public async Task GetNextSectionOfSongsInPlaylist()
        {
            for (int i =0; i < 10 && i < _songsInPlayList.Count(); i++)
            {
                await Downloader.DownloadSingleSong(_songsInPlayList.GetItemByIndex(i));
                _downloadedCount++;
            }
        }


        //UI update
        private void TimeElapsedHandler(object sender, ElapsedEventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                CurrentTime.Text = SongTimeFormat(_tickCount);
                TimeProgressBar.Progress = Convert.ToDouble(_tickCount) / (_mediaPlayer.Duration / 1000);
            });

            _tickCount++;
        }


        //Next Song 
        private async void OnCompleteHandler(object sender, EventArgs e)
        {
            _mediaPlayer.Reset();
            await Next();
        }

        private async void NextClicked(object sender, EventArgs e)
        {
            await Next();
        }

        public async Task Next()
        {
            _selectedItem = _songsInPlayList.Pop(); //get next song

            await PlaySelectedSong();
            if (_downloadedCount - _playedCount <= 2) //only 2 or less left on ready stack
            {
                await GetNextSectionOfSongsInPlaylist(); //get more songs 
            }
        }


        //Pause & play
        private void PlayPausedClicked(object sender, EventArgs e)
        {
            if (_mediaPlayer.IsPlaying)
            {
                _mediaPlayer.Pause();
                _timer.Stop();
            }
            else
            {
                _mediaPlayer.Start();
                _timer.Start();
            }
        }

        private async Task PlaySelectedSong()
        {
            _playedCount++;
            //set up media player for song
            if (_mediaPlayer.IsPlaying)
            {
                _mediaPlayer.Stop();
                _mediaPlayer.Reset();
            }
            var fileName = Directory.GetFiles(FILE_DIR).FirstOrDefault(x => Path.GetFileName(x).Contains(_selectedItem.Id));
            var filePath = Path.Combine(FILE_DIR, fileName);
            await _mediaPlayer.SetDataSourceAsync(filePath);
            _mediaPlayer.Prepare();
            _mediaPlayer.Start();
            _timer.Start();

            //update song info
            _tickCount = 0;
            Device.BeginInvokeOnMainThread(() =>
            {
                CurrentTime.Text = SongTimeFormat(_tickCount);
                TimeProgressBar.Progress = 0;
                SongTitle.Text = _selectedItem.Title;
                SongDuration.Text = SongTimeFormat(_mediaPlayer.Duration / 1000);
                SongImage.Source = _selectedItem.ImageSource;
            });
        }


        //helper methods
        public string SongTimeFormat(int seconds)
        {
            var timeSpanPlaying = TimeSpan.FromSeconds(seconds);
            return string.Format("{0:D1}:{1:D2}", timeSpanPlaying.Minutes, timeSpanPlaying.Seconds);
        }

        public string GetSongIdFromUrl(string url)
        {
            var query = HttpUtility.ParseQueryString(new Uri(url).Query);
            return query["v"];
        }

    }
}