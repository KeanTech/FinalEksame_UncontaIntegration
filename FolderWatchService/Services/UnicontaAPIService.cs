using Alaska.Library.Models.Service;
using Alaska.Library.Models.Uniconta.Userdefined;
using FolderWatchService.Core.Handlers;
using FolderWatchService.Core.Helpers;
using FolderWatchService.Core.Managers;
using FromXSDFile.OIOUBL.ExportImport.eDelivery;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Documents;
using Uniconta.API.Service;
using Uniconta.API.System;
using Uniconta.ClientTools.DataModel;
using Uniconta.Common;
using Uniconta.Common.User;
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

        public CrudAPI Api { get { return _api; } }
        public async Task<ErrorCodes> HandleFolderCreatedEvent(string filePath, string fileName)
        {
            ScannerFile scannerFile = ScannerFile.Factory(_api.CompanyEntity, fileName, filePath, "Not Created", "Uploaded");
            var insertResult = await _api.Insert(scannerFile);

            // If the result was not successful it will write to errorlog and return the ErrorCode 
            if (insertResult != ErrorCodes.Succes)
            {
                var task = ErrorHandler.WriteError(new UnicontaException("Error while trying to insert record", new UnicontaException($"Error on Insert in {nameof(UnicontaAPIService.HandleFolderCreatedEvent)}")), insertResult).ConfigureAwait(false);
                return insertResult;
            }

            // Read all lines from file 
            var fileLines = File.ReadAllLines(filePath);

            List<ScannerData> scannerDataList = new List<ScannerData>();
            var inventoryItems = await GetInventory();

            if (fileLines.Length > 0)
            {
                CreateScannerData(scannerDataList, scannerFile, fileLines, inventoryItems);
            }

            if (scannerDataList.Count > 0)
                insertResult = await _api.Insert(scannerDataList);

            if (insertResult != ErrorCodes.Succes)
            {
                var task = ErrorHandler.WriteError(new UnicontaException("Error while trying to insert record lines", new UnicontaException($"Error on Insert in {nameof(UnicontaAPIService.HandleFolderCreatedEvent)}")), insertResult).ConfigureAwait(false);
                return insertResult;
            }

            insertResult = await CreateAttachmentForScannerFile(scannerFile, filePath, fileName);

            if (insertResult != ErrorCodes.Succes)
            {
                var task = ErrorHandler.WriteError(new UnicontaException("Error while trying to insert attachment", new UnicontaException($"Error on Insert in {nameof(UnicontaAPIService.HandleFolderCreatedEvent)}")), insertResult).ConfigureAwait(false);
                return insertResult;
            }

            return ErrorCodes.Succes;
        }

        private List<ScannerData> CreateScannerData(List<ScannerData> data, ScannerFile scannerFile, string[] fileLines, InvItemClient[] inventoryItems) 
        {
            foreach (var line in fileLines)
            {
                string[] lineData = line.Split(';');
                var item = inventoryItems.FirstOrDefault(x => x._Item == lineData[0]);

                ScannerData scannerData = ScannerData.Factory(scannerFile);
                if (item == null)
                    scannerData.Status = $"Item: {lineData[0]} does not exist";
                else
                {
                    scannerData.ItemNumber = lineData[0];
                }

                int qty;

                if (int.TryParse(lineData[1], out qty))
                    scannerData.Quantity = qty;
                else
                    scannerData.Status = "Quantity cannot be 0";

                scannerData.Date = DateTime.Parse(lineData[2]);

                if (string.IsNullOrEmpty(scannerData.Status))
                    scannerData.Status = "Validated";

                data.Add(scannerData);
            }

            return data;
        }

        /// <summary>
        /// Returns a list of all <see cref="InvItem"/> 
        /// </summary>
        /// <returns></returns>
        public async Task<InvItemClient[]> GetInventory()
        {
            var invItems = await _api.Query<InvItemClient>();
            return invItems;
        }

        /// <summary>
        /// Creates a Uniconta API session with the information from the config 
        /// </summary>
        /// <returns>A new <see cref="CrudAPI"/></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<CrudAPI> Login(LoginInfo loginInfo)
        {
            UnicontaConnection connection = new UnicontaConnection(APITarget.Live);
            Session session = new Session(connection);

            // Get access to Uniconta Api through the session
            var loginResponse = await session.LoginAsync(

                LoginId: loginInfo.Username,
                Password: loginInfo.Password,
                loginProfile: LoginType.API,
                AccessIdent: new Guid(loginInfo.ApiKey)

            );

            // if the login is not successful
            if (loginResponse != ErrorCodes.Succes)
                throw new ArgumentException("Error while trying to login", new Exception($"Error Code: {loginResponse}"));

            var companyEntity = await session.GetCompany(loginInfo.CompanyId);

            if (companyEntity == null)
                throw new ArgumentNullException($"You do not have permission to use company {loginInfo.CompanyId}");

            _api = new CrudAPI(session, companyEntity);

            return _api;
        }


        public void Dispose()
        {
            _api = null;
        }

        private async Task<ErrorCodes> CreateAttachmentForScannerFile(ScannerFile scannerFile, string fullPath, string fileName)
        {
            UserDocsClient userDocsClient = new UserDocsClient();
            userDocsClient.SetMaster(scannerFile);
            userDocsClient._Data = File.ReadAllBytes(fullPath);
            if (fullPath.GetFileExtention() == "txt")
                userDocsClient.DocumentType = FileextensionsTypes.TXT;

            userDocsClient.Created = DateTime.Now;
            userDocsClient.Text = fileName;

            return await _api.Insert(userDocsClient);
        }
    }
}
