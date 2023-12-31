﻿using Alaska.Library.Core.Factories;
using Alaska.Library.Models.Service;
using Alaska.Library.Models.Uniconta.Inventory;
using Alaska.Library.Models.Uniconta.Userdefined;
using FolderWatchService.Core.Handlers;
using FolderWatchService.Core.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Uniconta.API.Inventory;
using Uniconta.API.Service;
using Uniconta.API.System;
using Uniconta.ClientTools.DataModel;
using Uniconta.Common;
using Uniconta.Common.User;
using Uniconta.DataModel;

namespace FolderWatchService.Services
{
    /// <summary>
    /// This class is responsible for communikation to the Uniconta API 
    /// </summary>
    public class UnicontaAPIService : IUnicontaAPIService
    {
        private CrudAPI _api;
        private readonly IUnicontaFactory _factory;
        private readonly IErrorHandler _errorHandler;

        public UnicontaAPIService(IUnicontaFactory factory, IErrorHandler errorHandler)
        {
            _factory = factory;
            _errorHandler = errorHandler;
        }

        /// <summary>
        /// Public getter for company
        /// <para>Used for access purposes and to create Uniconta objects</para>
        /// </summary>
        public Company Company => _api?.CompanyEntity;
        public ProductionAPI CreateProductionApi() => _factory.CreateProductionApi(_api);


        #region Query methods
        /// <summary>
        /// Used to make a query to the Uniconta Api where T is a UnicontaBaseEntity
        /// </summary>
        /// <typeparam name="T">The object has to implement UnicontaBaseEntity</typeparam>
        /// <returns>An array of the parsed generic type</returns>
        public async Task<T[]> Query<T>() where T : class, UnicontaBaseEntity, new()
        {
            T[] entity = await _api.Query<T>();
            return entity;
        }

        public async Task<T[]> Query<T>(List<PropValuePair> filters) where T : class, UnicontaBaseEntity, new()
        {
            T[] entity = await _api.Query<T>(filters);
            return entity;
        }

        public async Task<T[]> Query<T>(UnicontaBaseEntity master) where T : class, UnicontaBaseEntity, new()
        {
            T[] entity = await _api.Query<T>(master);
            return entity;
        }

        #endregion

        #region Insert methods

        public async Task<ErrorCodes> Insert(UnicontaBaseEntity entity)
        {
            return await _api.Insert(entity);
        }

        public async Task<ErrorCodes> Insert(IEnumerable<UnicontaBaseEntity> entities)
        {
            return await _api.Insert(entities);
        }

        #endregion

        #region Update methods
        public async Task<ErrorCodes> Update(UnicontaBaseEntity entity)
        {
            return await _api.Update(entity);
        }

        public async Task<ErrorCodes> Update(IEnumerable<UnicontaBaseEntity> entities)
        {
            return await _api.Update(entities);
        }

        #endregion

        #region HandleFolderEvent

        /// <summary>
        /// Used to handle folder event, it sends the data from file to Uniconta
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="fileName"></param>
        /// <returns>If it fails to send some of the data it will return the error</returns>
        public async Task<ErrorCodes> HandleFolderCreatedEvent(string filePath, string fileName)
        {
            // Uses the factory to create ScannerFile object
            ScannerFile scannerFile = _factory.Create<ScannerFile>();
            // Sets the master on the scannerFile 
            // Used in Uniconta to set the foreign key in the db table
            scannerFile.SetMaster(_api.CompanyEntity);
            scannerFile.KeyName = fileName;
            scannerFile.FilePath = filePath;
            scannerFile.Created = DateTime.Now;
            scannerFile.Status = "Initiated";

            // Insert the ScannerFile
            var insertResult = await Insert(scannerFile);

            // If the result was not successful it will write to errorlog and return the ErrorCode 
            if (insertResult != ErrorCodes.Succes)
            {
                // Wait for the ErrorHandler to write to log
                await _errorHandler.WriteError(new UnicontaException("Error while trying to insert record", new UnicontaException($"Error on Insert in {nameof(UnicontaAPIService.HandleFolderCreatedEvent)}")), insertResult);
                return insertResult;
            }

            // Read all lines from file 
            var fileLines = File.ReadAllLines(filePath);

            // Uses the factory to get a new empty list
            List<ScannerData> scannerDataList = _factory.CreateListOf<ScannerData>();
            
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
                insertResult = await Insert(scannerDataList);

            // if the insert fails write to log and return the error
            if (insertResult != ErrorCodes.Succes)
            {
                await _errorHandler.WriteError(new UnicontaException("Error while trying to insert record lines", new UnicontaException($"Error on Insert in {nameof(HandleFolderCreatedEvent)}")), insertResult);
                return insertResult;
            }

            // Add the file to the ScannerData as an attachment 
            insertResult = await CreateAttachmentFor(scannerFile, filePath, fileName);

            // if the insert fails write to log and return the error
            if (insertResult != ErrorCodes.Succes)
            {
                await _errorHandler.WriteError(new UnicontaException("Error while trying to insert attachment", new UnicontaException($"Error on Insert in {nameof(HandleFolderCreatedEvent)}")), insertResult);
                return insertResult;
            }

            return ErrorCodes.Succes;
        }

        #endregion

        #region Create ScannerData

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
                // If the string is empty theres no data to read.
                if (string.IsNullOrEmpty(line))
                    continue;

                // splits the line into an array
                string[] lineData = line.Split(';');

                // Verify that there is an item in Uniconta that has the same EAN-number 
                /// <see cref="InvItemClientUser.EANPallet"/> is a custom field in Uniconta to contain EAN-number
                var item = inventoryItems.FirstOrDefault(x => x.GetUserFieldString(nameof(InvItemClientUser.EANPallet)) == lineData[0]);

                // Create a new instance of ScannerData through the UnicontaFactory
                ScannerData scannerData = _factory.Create<ScannerData>();
                scannerData.SetMaster(scannerFile);
                // If the item does not exist give the line Status does not exist
                if (item == null)
                    scannerData.Status = $"Error: {lineData[0]} does not exist";
                // Else set the item 
                else
                {
                    // To create a production we need the item number aka item 
                    // Cant use EAN-Number because it a userdefined field
                    scannerData.ItemNumber = item.Item;
                }

                int qty;
                // Try get the quantity if it parses as a int and it is not 0 
                // Set the quantity
                if (int.TryParse(lineData[1], out qty) && qty != 0)
                    scannerData.Quantity = qty;
                // Else set status on line to Cannot be 0
                else
                    scannerData.Status = "Error: Quantity cannot be 0";
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

        #endregion

        /// <summary>
        /// Query the <see cref="InvItemClient"/> 
        /// </summary>
        /// <returns></returns>
        public async Task<InvItemClient[]> GetInventory()
        {
            var invItems = await Query<InvItemClient>();
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
            // Get a new Uniconta connection target live (production) 
            UnicontaConnection connection = new UnicontaConnection(APITarget.Live);
            // Create a session with the connection
            Session session = new Session(connection);

            ErrorCodes loginResponse = ErrorCodes.Succes;
            
            try
            {
                // Get access to Uniconta Api through the session
                /// login with the information parsed in <see cref="LoginInfo"/>
                loginResponse = await session.LoginAsync(

                    LoginId: loginInfo.Username,
                    Password: loginInfo.Password,
                    loginProfile: LoginType.API,
                    AccessIdent: new Guid(loginInfo.ApiKey)

                );
            }
            catch (UnicontaException ex)
            {
                await _errorHandler.WriteError(ex, loginResponse);
            }
            finally
            { // if the login is not successful
                if (loginResponse != ErrorCodes.Succes)
                {
                    _errorHandler.ShowErrorMessage($"Uniconta login failed cant start service!\nError Message: {loginResponse}");
                    await _errorHandler.WriteError(new UnicontaException($"Uniconta login failed cant start service"), loginResponse);
                    // Exit with exitcode: ERROR_SERVICE_LOGON_FAILED(1069) https://learn.microsoft.com/en-us/windows/win32/debug/system-error-codes--1000-1299-
                    // To make sure that the service terminates if login fails
                    Environment.Exit(1069);
                }
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
        private async Task<ErrorCodes> CreateAttachmentFor<T>(T master, string fullPath, string fileName) where T : class, UnicontaBaseEntity
        {
            UserDocsClient userDocsClient = new UserDocsClient();
            // Sets the extension.
            if (fullPath.GetFileExtention() == "txt")
                userDocsClient.DocumentType = FileextensionsTypes.TXT;
            else
                return ErrorCodes.IllegalFiletype;

            userDocsClient.SetMaster(master);
            // Reads the file as bytes
            userDocsClient._Data = File.ReadAllBytes(fullPath);

            userDocsClient.Created = DateTime.Now;
            userDocsClient.Text = fileName;

            // Wait for response and return the result
            return await Insert(userDocsClient);
        }
    }
}
