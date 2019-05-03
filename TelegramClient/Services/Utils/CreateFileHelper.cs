using System;
using System.IO;

namespace TelegramClient.Utils
{
    public static class CreateFileHelper
    {
        private readonly static String DirectoryPath = "files";

        public static void CreateDirectory()
        {
            if (!Directory.Exists(DirectoryPath))
                Directory.CreateDirectory(DirectoryPath);
        }

        public static void CreateFile(string fileName, string date)
        {
            CreateDirectory();

            string filePath = $"{DirectoryPath}/{fileName}";
            if (File.Exists(filePath)) File.Delete(filePath);

            using (StreamWriter w = File.AppendText(filePath))
            {
                w.Write(date);
            }
        }
    }
}
