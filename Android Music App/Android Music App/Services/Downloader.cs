using Android_Music_App.Models;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace Android_Music_App.Services
{
    public static class Downloader
    {
        private const string FILE_DIR = "/storage/emulated/0/Download/";
        public static async Task DownloadSingleSong(SearchResultsObject song)
        {
            if (!Directory.GetFiles(FILE_DIR).Any(x => Path.GetFileName(x).Contains(song.Id.ToString()))) //if not already downloaded
            {
                var youtube = new YoutubeClient();

                var streamManifest = await youtube.Videos.Streams.GetManifestAsync(song.Id);

                // get highest bitrate audio-only stream
                var streamInfo = streamManifest.GetAudioOnly().WithHighestBitrate();
                //var streamInfo = streamManifest.GetAudioOnly().OrderBy(x => x.Size).FirstOrDefault(); // get smallest audio file

                if (streamInfo != null)
                {
                    //var stream = await youtube.Videos.Streams.GetAsync(streamInfo); // Get the actual stream
                    // Download the stream to file
                    await youtube.Videos.Streams.DownloadAsync(streamInfo, Path.Combine(FILE_DIR, $"{song.Id}.{streamInfo.Container}"));
                }
            }
        }
    }
}
