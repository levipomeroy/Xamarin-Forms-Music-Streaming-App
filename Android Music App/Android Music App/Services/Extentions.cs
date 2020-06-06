using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Android_Music_App.Services
{
    static class Extentions
    {
        private static Random rng = new Random();

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static string CleanTitle(this string title)
        {
            //Remove parathesis 
            title = Regex.Replace(title, @"\([^()]*\)", string.Empty);
            //Convert to title case 
            title = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(title.ToLower());
            //remove emojis and junk 
            title = Regex.Replace(title, @"[^\u0000-\u007F]+", "");
            //standardize seperator 
            title = Regex.Replace(title, "[+|:]", " - ");
            //remove multiple spaces
            title = Regex.Replace(title, @"\s+", " ");

            return title;
        }

        public static string GetArtistName(this string title)
        {
            var artist = title.Substring(0, title.IndexOf('-'));

            return artist;
        }

    }
}
