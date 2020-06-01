using Android_Music_App.Models;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace Android_Music_App.Services
{
    public static class SongFileManager
    {
        private const string FILE_DIR = "/storage/emulated/0/MusicQueue/";
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

        public static void InitFolder()
        {
            Directory.CreateDirectory(FILE_DIR);
        }

        public static void CleanUpMusicFolder()
        {
            const int FILE_LIMIT = 300;

            var allFiles = Directory.GetFiles(FILE_DIR); //cheaper
            if (allFiles.Count() >= FILE_LIMIT)
            {
                var oldFiles = Directory.EnumerateFiles(FILE_DIR)
                        .Select(i => new FileInfo(i))
                        .OrderByDescending(i => i.CreationTime)
                        .Skip(FILE_LIMIT/2);

                foreach(var file in oldFiles)
                {
                    file.Delete();
                }
            }
        }

    }
}
