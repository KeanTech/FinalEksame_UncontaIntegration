using System.IO;
using System.Linq;

namespace FolderWatchService.Core.Helpers
{
    /// <summary>
    /// Helperclass for paths
    /// </summary>
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

        /// <summary>
        /// Check if the folderpath exist if not then create it
        /// </summary>
        /// <param name="folder1"></param>
        /// <param name="folder2"></param>
        /// <param name="folder3"></param>
        public static void CreateNeededFolders(string folder1, string folder2, string folder3) 
        {
            if (Directory.Exists(folder1) == false)
                Directory.CreateDirectory(folder1);

            if (Directory.Exists(folder2) == false)
                Directory.CreateDirectory(folder2);

            if (Directory.Exists(folder3) == false)
                Directory.CreateDirectory(folder3);
        }
    }
}
