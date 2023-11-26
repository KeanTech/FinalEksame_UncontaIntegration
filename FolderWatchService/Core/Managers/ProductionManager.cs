using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderWatchService.Core.Managers
{
    public class ProductionManager
    {
        public async Task HandleFolderEvent(object sender, FileSystemEventArgs e)
        {
            var fileNameArray = e.Name.Split('.');
            var fileType = fileNameArray.LastOrDefault();
            if (fileType != "txt")
                return;

            var fileName = e.FullPath.Split('\\').LastOrDefault();
            if (fileName == "log.txt")
                return;

            if (fileName == "error.txt")
                return;

            var path = e.FullPath.Substring(0, e.FullPath.Length - fileName.Length);
        }

    }
}
