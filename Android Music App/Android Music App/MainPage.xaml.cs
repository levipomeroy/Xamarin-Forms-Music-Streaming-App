using Android_Music_App.Models;
using Android_Music_App.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Xamarin.Forms;
using YouTubeSearch;

namespace Android_Music_App
{
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        private ObservableCollection<PlaylistObject> YoutubeSearchResults;

        public MainPage()
        {
            InitializeComponent();
            SongFileManager.InitFolder();
            SongFileManager.CleanUpMusicFolder();

            BindingContext = new SearchResultsObject();
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
            var item = (PlaylistObject)e.SelectedItem;

            await Navigation.PushModalAsync(new NavigationPage(new MediaPlayerPage(item)));
        }
    }
}
