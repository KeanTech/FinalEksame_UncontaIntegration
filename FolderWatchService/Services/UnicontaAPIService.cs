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

        /// <summary>
        /// Public get so we can get access to the api
        /// </summary>
        public CrudAPI Api { get { return _api; } }

        /// <summary>
        /// Used to handle folder event, it sends the data from file to Uniconta
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="fileName"></param>
        /// <returns>If it fails to send some of the data it will return the error</returns>
        public async Task<ErrorCodes> HandleFolderCreatedEvent(string filePath, string fileName)
        {
            // Uses the static factory to create ScannerFile object
            ScannerFile scannerFile = ScannerFile.Factory(_api.CompanyEntity, fileName, filePath, "Not Created", "Uploaded");
            // Insert the ScannerFile
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
            // All items 
            var inventoryItems = await GetInventory();

            // If theres lines in the file
            if (fileLines.Length > 0)
            {
                // Creates all the filelines as ScannerData
                CreateScannerData(scannerDataList, scannerFile, fileLines, inventoryItems);
            }

            // Insert scannerdata if theres any entries in the list
            if (scannerDataList.Count > 0)
                insertResult = await _api.Insert(scannerDataList);

            // if the insert fails write to log and return the error
            if (insertResult != ErrorCodes.Succes)
            {
                var task = ErrorHandler.WriteError(new UnicontaException("Error while trying to insert record lines", new UnicontaException($"Error on Insert in {nameof(UnicontaAPIService.HandleFolderCreatedEvent)}")), insertResult).ConfigureAwait(false);
                return insertResult;
            }

            // Add the file to the ScannerData as an attachment 
            insertResult = await CreateAttachmentForScannerFile(scannerFile, filePath, fileName);

            // if the insert fails write to log and return the error
            if (insertResult != ErrorCodes.Succes)
            {
                var task = ErrorHandler.WriteError(new UnicontaException("Error while trying to insert attachment", new UnicontaException($"Error on Insert in {nameof(UnicontaAPIService.HandleFolderCreatedEvent)}")), insertResult).ConfigureAwait(false);
                return insertResult;
            }

            return ErrorCodes.Succes;
        }

        /// <summary>
        /// Creates a <see cref="List{ScannerData}"/> from the fileLines
        /// </summary>
        /// <param name="data"></param>
        /// <param name="scannerFile"></param>
        /// <param name="fileLines"></param>
        /// <param name="inventoryItems"></param>
        /// <returns></returns>
        private List<ScannerData> CreateScannerData(List<ScannerData> data, ScannerFile scannerFile, string[] fileLines, InvItemClient[] inventoryItems)
        {
            foreach (var line in fileLines)
            {
                // splits the line into an array
                string[] lineData = line.Split(';');
                // Verify that the item exist in uniconta
                var item = inventoryItems.FirstOrDefault(x => x._Item == lineData[0]);

                ScannerData scannerData = ScannerData.Factory(scannerFile);

                // If the item does not exist give the line Status does not exist
                if (item == null)
                    scannerData.Status = $"Item: {lineData[0]} does not exist";
                // Else set the item 
                else
                {
                    scannerData.ItemNumber = lineData[0];
                }

                int qty;
                // Try get the quantity if it parses as a int and it is not 0 
                // Set the quantity
                if (int.TryParse(lineData[1], out qty) && qty != 0)
                    scannerData.Quantity = qty;
                // Else set status on line to Cannot be 0
                else
                    scannerData.Status = "Quantity cannot be 0";
                DateTime dateTime;
                // Try to parse the date string to a DateTime
                DateTime.TryParse(lineData[2], out dateTime);

                // If it could'nt parse the date it will be at its minimum value.
                // So if its not MinValue set the date  
                if (dateTime != DateTime.MinValue)
                    scannerData.Date = dateTime;
                // Else set to days date
                else
                    scannerData.Date = DateTime.Now;

                // Finaly check if theres any status on the ScannerData.
                // If not set the Status to validated.
                if (string.IsNullOrEmpty(scannerData.Status))
                    scannerData.Status = "Validated";

                // Add the entry to the list
                data.Add(scannerData);
            }

            // Return the list 
            return data;
        }

        /// <summary>
        /// Query the <see cref="InvItemClient"/> 
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
        public async Task<ErrorCodes> Login(LoginInfo loginInfo)
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
            { 
                throw new ArgumentException("Error while trying to login", new Exception($"Error Code: {loginResponse}"));
            }
            // Try to get company to see if the user i authenticated
            var companyEntity = await session.GetCompany(loginInfo.CompanyId);
            
            // If company entity is null the the user does not have access to it  
            if (companyEntity == null)
            { 
                throw new ArgumentNullException($"You do not have permission to use company {loginInfo.CompanyId}");
            }

            // Sets the internal api
            _api = new CrudAPI(session, companyEntity);

            return loginResponse;
        }

        /// <summary>
        /// Disposes the api
        /// </summary>
        public void Dispose()
        {
            _api = null;
        }

        /// <summary>
        /// Used to attach file to the entry
        /// </summary>
        /// <param name="scannerFile"></param>
        /// <param name="fullPath"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private async Task<ErrorCodes> CreateAttachmentForScannerFile(ScannerFile scannerFile, string fullPath, string fileName)
        {
            UserDocsClient userDocsClient = new UserDocsClient();
            userDocsClient.SetMaster(scannerFile);
            // Reads the file as bytes
            userDocsClient._Data = File.ReadAllBytes(fullPath);
            // Sets the extension.
            if (fullPath.GetFileExtention() == "txt")
                userDocsClient.DocumentType = FileextensionsTypes.TXT;

            userDocsClient.Created = DateTime.Now;
            userDocsClient.Text = fileName;

            // Wait for response and return the result
            return await _api.Insert(userDocsClient);
        }
    }
}
