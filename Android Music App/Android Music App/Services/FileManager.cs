using Android_Music_App.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace Android_Music_App.Services
{
    public static class FileManager
    {
        private const string BASE_DIR = "/storage/emulated/0/MusicApp/";
        private const string QUEUE_FOLDER = "Queue/";

        private const string LOG_FOLDER= "Logs/";
        private const string LOG_FILE_NAME = "logs.txt";

        private const string SAVED_FOLDER = "Saved/";
        private const string RECENTLY_PLAYED_PLAYLISTS_FILE_NAME = "RecentlyPlayedPlaylists.txt";
        private const string SAVED_SONGS = "SavedSongs.txt";

        public static void InitFolders()
        {
            Directory.CreateDirectory(BASE_DIR); //Create app folder
            Directory.CreateDirectory(BASE_DIR + QUEUE_FOLDER); // Create music queue folder
            Directory.CreateDirectory(BASE_DIR + LOG_FOLDER); // Create log folder
            Directory.CreateDirectory(BASE_DIR + SAVED_FOLDER); //Create saved folder for playlists and songs
        }

        
        //Music management methods
        public static async Task DownloadSingleSong(SearchResultsObject song)
        {
            try
            {
                if (!Directory.GetFiles(BASE_DIR + QUEUE_FOLDER).Any(x => Path.GetFileName(x).Contains(song.Id.ToString()))) //if not already downloaded
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
                        await youtube.Videos.Streams.DownloadAsync(streamInfo, Path.Combine(BASE_DIR + QUEUE_FOLDER, $"{song.Id}.{streamInfo.Container}"));
                    }
                }
            }
            catch(Exception ex)
            {
                LogError($"Error downloading song: {song.Id}", ex);
            }
        }

        public static void CleanUpMusicFolder()
        {
            const int FILE_LIMIT = 300;

            var allFiles = Directory.GetFiles(BASE_DIR + QUEUE_FOLDER); //cheaper
            if (allFiles.Count() >= FILE_LIMIT)
            {
                var oldFiles = Directory.EnumerateFiles(BASE_DIR + QUEUE_FOLDER)
                        .Select(i => new FileInfo(i))
                        .OrderByDescending(i => i.CreationTime)
                        .Skip(FILE_LIMIT/2);

                foreach(var file in oldFiles)
                {
                    file.Delete();
                }
            }
        }

        
        //Logging methods 
        public static void LogInfo(string message)
        {
            var fullPath = BASE_DIR + LOG_FOLDER + LOG_FILE_NAME;
            if (!File.Exists(fullPath))
            {
                File.Create(fullPath);
                TextWriter tw = new StreamWriter(fullPath);
                tw.WriteLine($"\n{DateTime.Now}\t {message}");
                tw.Close();
            }
            else if (File.Exists(fullPath))
            {
                using (var tw = new StreamWriter(fullPath, true))
                {
                    tw.WriteLine($"\n{DateTime.Now}\t {message}");
                }
            }
        }

        public static void LogError(string message, Exception ex)
        {
            var fullPath = BASE_DIR + LOG_FOLDER + LOG_FILE_NAME;
            if (!File.Exists(fullPath))
            {
                File.Create(fullPath);
                TextWriter tw = new StreamWriter(fullPath);
                tw.WriteLine($"\n{DateTime.Now}\t {message}");
                tw.WriteLine($"Exception message: {ex.Message}");
                tw.WriteLine($"Exception stack trace: {ex.StackTrace}");
                tw.Close();
            }
            else if (File.Exists(fullPath))
            {
                using (var tw = new StreamWriter(fullPath, true))
                {
                    tw.WriteLine($"\n{DateTime.Now}\t {message}");
                    tw.WriteLine($"Exception message: {ex.Message}");
                    tw.WriteLine($"Exception stack trace: {ex.StackTrace}");
                }
            }
        }

        
        //Recently Played playlist methods
        public static void AddPlaylist(PlaylistObject sel)
        {
            var fullPath = BASE_DIR + SAVED_FOLDER + RECENTLY_PLAYED_PLAYLISTS_FILE_NAME;

            var playlists = GetPlaylists();
            if (playlists.Any(x => x.Id == sel.Id))  //remove if already in list and readd to refresh spot in list
            {
                playlists.Remove(playlists.FirstOrDefault(x => x.Id == sel.Id));
            }

            if(playlists.Count >= 20) //keep 20 last played only
            {
                playlists.Remove(playlists.LastOrDefault());
            }

            sel.Title = sel.Title.CleanTitle(); // clean it before writing it
            playlists.Insert(0, sel); //add playlist to beginning of list 
            var serializedPlaylists = JsonConvert.SerializeObject(playlists);

            File.WriteAllText(fullPath, serializedPlaylists);
        }

        public static List<PlaylistObject> GetPlaylists()
        {
            var fullPath = BASE_DIR + SAVED_FOLDER + RECENTLY_PLAYED_PLAYLISTS_FILE_NAME;

            if (!File.Exists(fullPath)) //no file - no point in trying to read
            {
                return new List<PlaylistObject>();
            }

            var jsonString = File.ReadAllText(fullPath);
            var recentlyPlayedPlaylists = JsonConvert.DeserializeObject<List<PlaylistObject>>(jsonString);

            return recentlyPlayedPlaylists ?? new List<PlaylistObject>();
        }


        //Saved songs methods
        public static void SaveSong(SearchResultsObject song)
        {
            var fullPath = BASE_DIR + SAVED_FOLDER + SAVED_SONGS;

            var savedSongs = GetSavedSongs();
            if (!savedSongs.Any(x => x.Id == song.Id))  //if not already saved
            {
                song.Title = song.Title.CleanTitle();  // clean it before writing it
                savedSongs.Insert(0, song); //add newly liked songs to beggining
            }

            var serializedPlaylists = JsonConvert.SerializeObject(savedSongs);

            File.WriteAllText(fullPath, serializedPlaylists);
        }

        public static List<SearchResultsObject> GetSavedSongs()
        {
            var fullPath = BASE_DIR + SAVED_FOLDER + SAVED_SONGS;

            if (!File.Exists(fullPath)) //no file - no point in trying to read
            {
                return new List<SearchResultsObject>();
            }

            var jsonString = File.ReadAllText(fullPath);
            var savedSongs = JsonConvert.DeserializeObject<List<SearchResultsObject>>(jsonString);

            return savedSongs ?? new List<SearchResultsObject>();
        }

        public static bool IsSongSaved(SearchResultsObject song)
        {
            var savedSongs = GetSavedSongs();

            return savedSongs.Any(x => x.Id == song.Id);
        }

        public static void UnSaveSong(SearchResultsObject song)
        {
            var fullPath = BASE_DIR + SAVED_FOLDER + SAVED_SONGS;

            var savedSongs = GetSavedSongs();
            if (savedSongs.Any(x => x.Id == song.Id))  //if already saved
            {
                savedSongs.Remove(savedSongs.FirstOrDefault(x => x.Id == song.Id));
            }

            var serializedPlaylists = JsonConvert.SerializeObject(savedSongs);

            File.WriteAllText(fullPath, serializedPlaylists);
        }
    }
}
