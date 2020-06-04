using Android_Music_App.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Android_Music_App.Services
{
    public static class RecentlyPlayed
    {

        private const string FILE_DIR = "/storage/emulated/0/MusicApp/RecentlyPlayed/";
        private const string FILE_NAME = "RecentlyPlayedPlaylists.txt";
        public static void AddPlaylist(PlaylistObject sel)
        {
            Directory.CreateDirectory(FILE_DIR);
            var fullPath = FILE_DIR + FILE_NAME;

            var playlists = GetPlaylists();
            if (playlists.Any(x => x.Id == sel.Id))  //remove if already in list and readd to refresh spot in list
            {
                playlists.Remove(playlists.FirstOrDefault(x => x.Id == sel.Id));
            }
            playlists.Insert(0, sel); //add playlist to beginning of list 
            var serializedPlaylists = JsonConvert.SerializeObject(playlists);

            File.WriteAllText(fullPath, serializedPlaylists);
        }

        public static List<PlaylistObject> GetPlaylists()
        {
            var fullPath = FILE_DIR + FILE_NAME;

            if (!File.Exists(fullPath)) //no file - no point in trying to read
            {
                return new List<PlaylistObject>();
            }

            var jsonString = File.ReadAllText(fullPath);
            var recentlyPlayedPlaylists = JsonConvert.DeserializeObject<List<PlaylistObject>>(jsonString);

            return recentlyPlayedPlaylists ?? new List<PlaylistObject>();
        }
    }
}
