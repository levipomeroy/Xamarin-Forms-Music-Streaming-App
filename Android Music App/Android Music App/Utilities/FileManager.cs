using Android_Music_App.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Android_Music_App.Services
{
    public static class FileManager
    {
        private const string BASE_DIR = "/storage/emulated/0/MusicApp/";

        private const string LOG_FOLDER= "Logs/";
        private const string LOG_FILE_NAME = "logs.txt";

        private const string SAVED_FOLDER = "Saved/";
        private const string RECENTLY_PLAYED_PLAYLISTS_FILE_NAME = "RecentlyPlayedPlaylists.txt";
        private const string SAVED_SONGS = "SavedSongs.txt";

        public static void InitFolders()
        {
            Directory.CreateDirectory(BASE_DIR); //Create app folder
            Directory.CreateDirectory(BASE_DIR + LOG_FOLDER); // Create log folder
            Directory.CreateDirectory(BASE_DIR + SAVED_FOLDER); //Create saved folder for playlists and songs
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
                    tw.Close();
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
                    tw.Close();
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

            sel.CleanTitle();
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
        public static void SaveSong(Song song)
        {
            var fullPath = BASE_DIR + SAVED_FOLDER + SAVED_SONGS;

            var savedSongs = GetSavedSongs();
            if (!savedSongs.Any(x => x.Id == song.Id))  //if not already saved
            {
                savedSongs.Insert(0, song); //add newly liked songs to beggining
            }

            var serializedPlaylists = JsonConvert.SerializeObject(savedSongs);

            File.WriteAllText(fullPath, serializedPlaylists);
        }

        public static List<Song> GetSavedSongs()
        {
            var fullPath = BASE_DIR + SAVED_FOLDER + SAVED_SONGS;

            if (!File.Exists(fullPath)) //no file - no point in trying to read
            {
                return new List<Song>();
            }

            var jsonString = File.ReadAllText(fullPath);
            var savedSongs = JsonConvert.DeserializeObject<List<Song>>(jsonString);

            return savedSongs ?? new List<Song>();
        }

        public static bool IsSongSaved(Song song)
        {
            var savedSongs = GetSavedSongs();

            return savedSongs.Any(x => x.Id == song.Id);
        }

        public static void UnSaveSong(Song song)
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
