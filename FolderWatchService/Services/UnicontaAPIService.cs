using Alaska.Library.Models.Uniconta.Userdefined;
using FolderWatchService.Core.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uniconta.API.Service;
using Uniconta.API.System;
using Uniconta.ClientTools.DataModel;
using Uniconta.Common;
using Uniconta.DataModel;
using static Uniconta.API.System.CrudAPI;

namespace FolderWatchService.Services
{
    /// <summary>
    /// This class is responsible for communikation to the Uniconta API 
    /// </summary>
    public class UnicontaAPIService : IUnicontaAPIService
    {
        private CrudAPI _api;
        private readonly EncryptionManager _encryptionManager;

        public UnicontaAPIService(EncryptionManager encryptionManager)
        {
            _encryptionManager = encryptionManager;
        }

        /// <summary>
        /// Creates new entries in the userdefined tables <see cref="ScannerFile"/> and <see cref="ScannerData"/>
        /// 
        /// <para>
        /// If <see cref="ScannerFile"/> already exist in the database it will return <see cref="ErrorCodes.RecordExists"/>
        /// </para>
        /// </summary>
        /// <param name="scannerFile"></param>
        /// <param name="scannerData"></param>
        /// <returns>Error code <see cref="ErrorCodes.UnknownAppIdent"/> if <see cref="Login"/> was not called first</returns>
        public async Task<ErrorCodes> Create(ScannerFile scannerFile, List<ScannerData> scannerData)
        {
            if (_api == null)
                return ErrorCodes.UnknownAppIdent;

            await _api.Read(scannerFile);

            if (string.IsNullOrEmpty(scannerFile.KeyStr))
            {
                return ErrorCodes.RecordExists;
            }

            var insertResult = await _api.Insert(scannerFile);
            if (insertResult != ErrorCodes.Succes)
                return insertResult;

            return await _api.Insert(scannerData);
        }

        /// <summary>
        /// Updates the userdefined tables <see cref="ScannerFile"/> and <see cref="ScannerData"/>
        /// </summary>
        /// <param name="scannerFile"></param>
        /// <param name="scannerData"></param>
        /// <returns>Error code <see cref="ErrorCodes.UnknownAppIdent"/> if <see cref="Login"/> was not called first</returns>
        public async Task<ErrorCodes> Update(ScannerFile scannerFile, List<ScannerData> scannerData)
        {
            if (_api == null)
                return ErrorCodes.UnknownAppIdent;

            var updateResult = await _api.Update(scannerData);
            if (updateResult != ErrorCodes.Succes)
                return updateResult;

            return await _api.Update(scannerFile);
        }

        /// <summary>
        /// Returns a list of all <see cref="InvItem"/> 
        /// </summary>
        /// <returns></returns>
        public async Task<List<InvItem>> GetInventory()
        {
            var invItems = await _api.Query<InvItem>();
            return invItems.ToList();
        }

        /// <summary>
        /// Creates a Uniconta API session with the information from the config 
        /// </summary>
        /// <returns>A new <see cref="CrudAPI"/></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<CrudAPI> Login()
        {
            UnicontaConnection connection = new UnicontaConnection(APITarget.Live);
            Session session = new Session(connection);

            var settings = _encryptionManager.DecryptAppSetting();
            var loggedIn = await session.LoginAsync(settings[0], settings[1], Uniconta.Common.User.LoginType.API, new Guid(settings[2]));

            if (loggedIn != ErrorCodes.Succes)
                throw new ArgumentException("Error while trying to login");

            var companyEntity = session.GetCompany(settings[3]).GetAwaiter().GetResult();

            if (companyEntity == null)
                throw new ArgumentNullException($"You do not have permission to use company {settings}");

            _api = new CrudAPI(session, companyEntity);

            return _api;
        }

        public void Dispose()
        {
            _api = null;
        }
    }
}
