using Alaska.Library.Models.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderWatchService.Core.Managers
{
    public interface IConfigManager : IDisposable
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

        /// <summary>
        /// Used to get the information for login into UnicontaAPI
        /// </summary>
        /// <returns></returns>
        LoginInfo GetLoginInfo();
    }
}
