using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Search;
using YouTubeSearch;

namespace Android_Music_App.Services
{
    public class YoutubeSearcher
    {
        //YouTubeService _youtubeService;
        
        //public YoutubeSearcher()
        //{
        //    _youtubeService = new YouTubeService(new BaseClientService.Initializer()
        //    {
        //        ApiKey = "",
        //        ApplicationName = "MyApp"
        //    });
        //}

        //public async Task<List<SearchResult>> GetYoutubeResultsFromGoogleApi(string searchString)
        //{
        //    var searchListRequest = _youtubeService.Search.List("snippet");
        //    searchListRequest.Q = searchString; // Replace with your search term.
        //    searchListRequest.MaxResults = 20;                    

        //    var searchListResponse = await searchListRequest.ExecuteAsync();

        //    return searchListResponse.Items.Where(x => x.Id.Kind == "youtube#video").ToList();
        //}

        //public async Task<List<SearchResult>> GetRelatedSong(string videoId)
        //{
        //    var searchListRequest = _youtubeService.Search.List("snippet");
        //    searchListRequest.Type = "video";
        //    searchListRequest.RelatedToVideoId = videoId;

        //    var searchListResponse = await searchListRequest.ExecuteAsync();

        //    return searchListResponse.Items.ToList();
        //}

        //public static async Task YoutubeExplodeTest(string searchString)
        //{
        //    var youtube = new YoutubeClient().Search;
        //    var result = await youtube.GetVideosAsync(searchString);

        //}

        //public static async Task<List<PlaylistSearchComponents>> YoutubePlaylistSearcher(string searchString)
        //{
        //    var playlistClient = new PlaylistSearch();
        //    return await playlistClient.GetPlaylists(searchString, 1);
            
        //}

        //public static async Task<List<PlaylistItemsSearchComponents>> GetSongsInPlayList(string playListUrl)
        //{
        //    var songsInPlayList = new PlaylistItemsSearch();
        //    return await songsInPlayList.GetPlaylistItems(playListUrl);
        //}

    }
}
