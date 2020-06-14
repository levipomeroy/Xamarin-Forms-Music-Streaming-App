using Android.Media;
using Android_Music_App.Models;
using Android_Music_App.Services;
using AngleSharp.Common;
using Java.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Web;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XFGloss;
using YouTubeSearch;

namespace Android_Music_App
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MediaPlayerPage : ContentPage
    {
        //PlaylistObject _selectedPlayList;
        SearchResultsObject _selectedItem;
        Stack<SearchResultsObject> _songsInPlayList;
        MediaPlayer _mediaPlayer;
        Timer _timer;
        int _tickCount;
        int _downloadedCount;
        int _playedCount;
        private const string FILE_DIR = "/storage/emulated/0/MusicApp/Queue/";

        public MediaPlayerPage(/*PlaylistObject selectedItem, */Stack<SearchResultsObject> knownPlaylist)
        {
            try
            {
                InitializeComponent();
                //_selectedPlayList = selectedItem;
                _mediaPlayer = _mediaPlayer ?? new MediaPlayer();
                _songsInPlayList = new Stack<SearchResultsObject>();

                _tickCount = 0;
                _timer = new Timer();
                _timer.Interval = 1000; // 1 second
                _timer.Elapsed += TimeElapsedHandler;

                _mediaPlayer.Looping = false;
                _mediaPlayer.Completion += OnCompleteHandler;

                //if (knownPlaylist == null)
                //{
                //    InitPlaylist();
                //}
                //else
                //{
                    StartKnownPlaylist(knownPlaylist);
                //}  
            }
            catch (Exception ex)
            {
                FileManager.LogError("Generic error from media player page", ex);
            }
        }

        //Set up
        //public async void InitPlaylist()
        //{
        //    try
        //    {
        //        await GetSongsInPlaylist(_selectedPlayList.Url);
        //        await GetFirstSong();
        //        await GetNextSectionOfSongsInPlaylist();
        //    }
        //    catch (Exception ex)
        //    {
        //        FileManager.LogError("Error setting up media player page", ex);
        //    }
        //}

        public async void StartKnownPlaylist(Stack<SearchResultsObject> knownPlaylist)
        {
            _songsInPlayList = knownPlaylist;
            _selectedItem = _songsInPlayList.FirstOrDefault();
            if(_selectedItem == null)
            {
                FileManager.LogInfo("No songs in selected playlist at point of 'StartKnownPlaylist'");
            }

            //await FileManager.DownloadSingleSong(_selectedItem);
            _downloadedCount++;
            await PlaySelectedSong();

            _songsInPlayList.Pop(); //already set to play

            await GetNextSectionOfSongsInPlaylist();
        }

        //private async Task GetSongsInPlaylist(string playListUrl)
        //{
        //    var songsInPlayListClient = new PlaylistItemsSearch();
        //    var songs = await songsInPlayListClient.GetPlaylistItems(playListUrl);
        //    songs.Shuffle();
        //    _songsInPlayList = new Stack<SearchResultsObject>(songs.Select(x => new SearchResultsObject(x.getTitle(), x.getThumbnail(), GetSongIdFromUrl(x.getUrl()))));
        //}

        //private async Task GetFirstSong()
        //{
        //    _selectedItem = _songsInPlayList.FirstOrDefault();
        //    await FileManager.DownloadSingleSong(_selectedItem);
        //    _downloadedCount++;
        //    await PlaySelectedSong();

        //    _songsInPlayList.Pop(); //already set to play
        //}



        //Handlers 
        private void TimeElapsedHandler(object sender, ElapsedEventArgs e)
        {
            try
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    CurrentTime.Text = SongTimeFormat(_tickCount);
                    TimeProgressBar.Progress = Convert.ToDouble(_tickCount) / (_mediaPlayer.Duration / 1000);
                });

                _tickCount++;
            }
            catch (Exception ex)
            {
                FileManager.LogError("Error updating the timer and progress UI", ex);
            }
        }

        private async void OnCompleteHandler(object sender, EventArgs e)
        {
            _mediaPlayer.Reset();
            await Next();
        }

        private async void NextClicked(object sender, EventArgs e)
        {
            await Next();
        }

        private void PlayPausedClicked(object sender, EventArgs e)
        {
            if (_mediaPlayer.IsPlaying)
            {
                _mediaPlayer.Pause();
                _timer.Stop();

                PlayOrPauseButton.Text = "\U000f040c"; //play button
            }
            else
            {
                _mediaPlayer.Start();
                _timer.Start();

                PlayOrPauseButton.Text = "\U000f03e5"; //pause button
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            _mediaPlayer.Stop();
            _timer.Stop();
            _mediaPlayer.Reset();
            _mediaPlayer.Release();
        }

        private void SaveSongClicked(object sender, EventArgs e)
        {
            if(FileManager.IsSongSaved(_selectedItem))
            {
                FileManager.UnSaveSong(_selectedItem);
                HeartSong.Text = "\U000f02d5"; //heart outline
            }
            else
            {
                FileManager.SaveSong(_selectedItem);
                HeartSong.Text = "\U000f02d1"; //filled in heart
            }
        }

        private void RestartClicked(object sender, EventArgs e)
        {
            _mediaPlayer.SeekTo(0);
            _tickCount = 0;

            Device.BeginInvokeOnMainThread(() =>
            {
                CurrentTime.Text = SongTimeFormat(_tickCount);
                TimeProgressBar.Progress = Convert.ToDouble(_tickCount) / (_mediaPlayer.Duration / 1000);
            });

            if (_mediaPlayer.IsPlaying)
            {
                _mediaPlayer.Start();
            }
        }


        //support
        public async Task Next()
        {
            try
            {
                _timer.Stop();
                _selectedItem = _songsInPlayList.Pop(); //get next song

                await PlaySelectedSong();
                if (_downloadedCount - _playedCount <= 3) //only 3 or less left on ready stack
                {
                    await GetNextSectionOfSongsInPlaylist(); //get more songs 
                }
            }
            catch (Exception ex)
            {
                FileManager.LogError("Error getting next song", ex);
            }
        }

        private async Task PlaySelectedSong()
        {
            try
            {
                //set up media player for song
                if (_mediaPlayer.IsPlaying)
                {
                    _mediaPlayer.Stop();
                }
                _mediaPlayer.Reset();

                var fileName = Directory.GetFiles(FILE_DIR).FirstOrDefault(x => Path.GetFileName(x).Contains(_selectedItem.Id));
                var filePath = Path.Combine(FILE_DIR, fileName);

                await _mediaPlayer.SetDataSourceAsync(filePath);
                _mediaPlayer.Prepare();
                _mediaPlayer.Start();
                _playedCount++;
                _timer.Start();

                //update song info
                _tickCount = 0;
                Device.BeginInvokeOnMainThread(() =>
                {
                    CurrentTime.Text = SongTimeFormat(_tickCount);
                    TimeProgressBar.Progress = 0;

                    if (!string.IsNullOrWhiteSpace(_selectedItem.Artist))
                    {
                        Artist.Text = _selectedItem.Artist;
                        Artist.IsVisible = true;
                    }
                    SongTitle.Text = _selectedItem.Title.Replace($"{_selectedItem.Artist}-", string.Empty).CleanTitle();
                    SongDuration.Text = SongTimeFormat(_mediaPlayer.Duration / 1000);
                    SongImage.Source = _selectedItem.ImageSource;
                    PlayOrPauseButton.Text = "\U000f03e5"; //pause button
                    if (FileManager.IsSongSaved(_selectedItem))                 ///////////////// <-------------- This list should really be in memory and avoid this IO for every song
                    {
                        HeartSong.Text = "\U000f02d1"; //filled in heart
                    }
                    else
                    {
                        HeartSong.Text = "\U000f02d5"; //heart outline
                    }
                });
            }
            catch (Exception ex)
            {
                FileManager.LogError("Error playing selected song", ex);
            }
        }

        public async Task GetNextSectionOfSongsInPlaylist()
        {
            for (int i = 0; i < 10 && i < _songsInPlayList.Count(); i++)
            {
                var song = _songsInPlayList.GetItemByIndex(i);
                await FileManager.DownloadSingleSong(song);

                var trackInfo = Itunes.GetDataFromItunes(song, 1);
                _songsInPlayList.GetItemByIndex(i).Title = trackInfo.Title;
                _songsInPlayList.GetItemByIndex(i).ImageSource = trackInfo.ImageSource;
                _songsInPlayList.GetItemByIndex(i).Artist = trackInfo.Artist;

                _downloadedCount++;
            }
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