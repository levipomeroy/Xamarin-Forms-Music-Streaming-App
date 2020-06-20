using Android.Media;
using Android_Music_App.Models;
using Android_Music_App.Services;
using AngleSharp.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Android_Music_App
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MediaPlayerPage : ContentPage
    {
        Song _selectedItem;
        List<Song> _songsInPlayList;
        MediaPlayer _mediaPlayer;
        Timer _timer;
        int _tickCount;
        int _playedCount;
        int _infoGatheredCount;
        private const string FILE_DIR = "/storage/emulated/0/MusicApp/Queue/";

        public MediaPlayerPage(List<Song> knownPlaylist)
        {
            try
            {
                _mediaPlayer = new MediaPlayer();
                _mediaPlayer.Completion += OnCompleteHandler;

                _tickCount = 0;
                _timer = new Timer();
                _timer.Interval = 1000; // 1 second
                _timer.Elapsed += TimeElapsedHandler;

                StartKnownPlaylist(knownPlaylist);
            }
            catch (Exception ex)
            {
                FileManager.LogError("Generic error from media player page", ex);
            }
            InitializeComponent();
        }

        public async void StartKnownPlaylist(List<Song> knownPlaylist)
        {
            _infoGatheredCount++; // this one was done before page load
            _songsInPlayList = knownPlaylist;
            _selectedItem = _songsInPlayList.FirstOrDefault();
            if(_selectedItem == null)
            {
                FileManager.LogInfo("No songs in selected playlist at point of 'StartKnownPlaylist'");
            }

            await PlaySelectedSong().ConfigureAwait(false);
            await GetNextSectionOfSongsInPlaylist().ConfigureAwait(false);
        }

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
            await Next().ConfigureAwait(false);
        }

        private async void NextClicked(object sender, EventArgs e)
        {
            await Next().ConfigureAwait(false);
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

            Navigation.PushModalAsync(new NavigationPage(new MainPage()));
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

                //After all played, shuffle and start over
                if (_playedCount == _songsInPlayList.Count)
                {
                    _songsInPlayList.Shuffle();
                    _playedCount = 0;
                }

                _selectedItem = _songsInPlayList.GetItemByIndex(_playedCount);

                await PlaySelectedSong().ConfigureAwait(false);

                if ((_infoGatheredCount - _playedCount) <= 3) //only 3 or less ready, get more
                {
                    await GetNextSectionOfSongsInPlaylist().ConfigureAwait(false);
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
                var isSavedSong = FileManager.IsSongSaved(_selectedItem);  ///////////////// <-------------- This list should really be in memory and avoid this IO for every song, put on song instance as property with getter

                if (string.IsNullOrEmpty(_selectedItem.StreamUrl))
                {
                    _selectedItem.GetDataFromItunes();
                    await _selectedItem.GetStream().ConfigureAwait(false);
                }

                //set up media player for song
                if (_mediaPlayer.IsPlaying)
                {
                    _mediaPlayer.Stop();
                }
                _mediaPlayer.Reset();
                _mediaPlayer = new MediaPlayer();
                _mediaPlayer.Completion += OnCompleteHandler;
                _mediaPlayer.Prepared += (s, e) =>
                {
                    //update song info UI
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        CurrentTime.Text = SongTimeFormat(0);
                        TimeProgressBar.Progress = 0;

                        if (!string.IsNullOrWhiteSpace(_selectedItem.Artist))
                        {
                            Artist.Text = _selectedItem.Artist;
                            Artist.IsVisible = true;
                            _selectedItem.Title = _selectedItem.Title.Replace($"{_selectedItem.Artist} -", string.Empty).Replace($"{_selectedItem.Artist}", string.Empty);
                        }
                        SongTitle.Text = _selectedItem.Title;
                        SongDuration.Text = SongTimeFormat(_mediaPlayer.Duration / 1000);
                        SongImage.Source = _selectedItem.ImageSource;
                        PlayOrPauseButton.Text = "\U000f03e5"; //pause button
                        if (isSavedSong)
                        {
                            HeartSong.Text = "\U000f02d1"; //filled in heart
                        }
                        else
                        {
                            HeartSong.Text = "\U000f02d5"; //heart outline
                        }
                    });

                    if (_mediaPlayer.IsPlaying)
                    {
                        _mediaPlayer.Stop();
                    }
                    _mediaPlayer.Start();
                    _playedCount++;

                    _timer.Start();
                };

                _mediaPlayer.SetDataSource(_selectedItem.StreamUrl);
                _mediaPlayer.PrepareAsync();

                _tickCount = 0;
            }
            catch (Exception ex)
            {
                FileManager.LogError($"Error playing selected song, Title: {_selectedItem.Title}, Artist: {_selectedItem.Artist}, Id: {_selectedItem.Id}", ex);
                _playedCount++;
                await Next().ConfigureAwait(false);
            }
        }

        public async Task GetNextSectionOfSongsInPlaylist()
        {
            for (int i = _infoGatheredCount; i <= _infoGatheredCount + 10 && i < _songsInPlayList.Count(); i++)
            {
                //FileManager.LogInfo($"index: {i} info being gathered");
                _songsInPlayList.GetItemByIndex(i).GetDataFromItunes();
                await _songsInPlayList.GetItemByIndex(i).GetStream();

                _infoGatheredCount++;
            }
        }


        //helper methods
        public string SongTimeFormat(int seconds)
        {
            var timeSpanPlaying = TimeSpan.FromSeconds(seconds);
            return string.Format("{0:D1}:{1:D2}", timeSpanPlaying.Minutes, timeSpanPlaying.Seconds);
        }
    }
}