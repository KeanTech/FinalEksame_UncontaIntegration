using System.IO;
using System.Linq;

namespace FolderWatchService.Core.Helpers
{
    public static class FileAndPathHelper
    {
        /// <summary>
        /// Used to get the file extension from the full path 
        /// </summary>
        /// <param name="fullPath">Has to be the absolute path to the file</param>
        /// <returns>File extension <para></para>C:\user\files\file.txt = txt</returns>
        public static string GetFileExtention(this string fullPath)
        {
            var fileNameArray = fullPath.Split('.');
            return fileNameArray.LastOrDefault();
        }

        public static void CreateNeededFolders(string folder1, string folder2) 
        {
            if (Directory.Exists(folder1) == false)
                Directory.CreateDirectory(folder1);

            if (Directory.Exists(folder2) == false)
                Directory.CreateDirectory(folder2);
        }

        public static void MoveFileToPending(string fullPath) 
        {
            
        }
        public static void MoveFileToRead(string fullPath) 
        {
            
        }
    }
}
