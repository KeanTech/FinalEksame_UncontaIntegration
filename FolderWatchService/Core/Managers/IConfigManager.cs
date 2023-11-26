using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderWatchService.Core.Managers
{
    public interface IConfigManager
    {
        /// <summary>
        /// Used to get access to the App.config parse the name of the key
        /// </summary>
        /// <param name="key"></param>
        /// <returns>The value of the config</returns>
        string GetConfigFor(string key);
        
        /// <summary>
        /// Used to Read the App.config file ment to be stored somewhere
        /// </summary>
        void ReadConfigurations();
    }
}
