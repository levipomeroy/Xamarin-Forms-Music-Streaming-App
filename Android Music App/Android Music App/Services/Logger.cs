using Android.Text.Format;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Android_Music_App.Services
{
    public static class Logger
    {
        private const string LOG_FILE_PATH = "/storage/emulated/0/MusicApp/Logs/";
        private const string LOG_FILE_NAME = "logs.txt";
        public static void Info(string message)
        {
            Directory.CreateDirectory(LOG_FILE_PATH);

            var fullPath = LOG_FILE_PATH + LOG_FILE_NAME;
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

        public static void Error(string message, Exception ex)
        {
            Directory.CreateDirectory(LOG_FILE_PATH);

            var fullPath = LOG_FILE_PATH + LOG_FILE_NAME;
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
    }
}
