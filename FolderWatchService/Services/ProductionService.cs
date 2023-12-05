using Alaska.Library.Core.Factories;
using Alaska.Library.Models.Uniconta.Userdefined;
using FolderWatchService.Core.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uniconta.API.Inventory;
using Uniconta.API.Service;
using Uniconta.ClientTools.DataModel;
using Uniconta.Common;
using Uniconta.DataModel;

namespace FolderWatchService.Services
{
    public class ProductionService : IProductionService
    {
        private readonly IUnicontaAPIService _unicontaAPIService;
        private readonly IErrorHandler _errorHandler;

        public ProductionService(IUnicontaAPIService unicontaAPIService, IErrorHandler errorHandler)
        {
            _unicontaAPIService = unicontaAPIService;
            _errorHandler = errorHandler;
        }

        /// <summary>
        /// Used to create a list of production one at a time to determine which productions that has errors
        /// </summary>
        /// <param name="productions"></param>
        /// <param name="scannerData"></param>
        /// <param name="scannerFile"></param>
        /// <returns></returns>
        public async Task<ErrorCodes> CreateProductions(List<ProductionOrderClient> productions, ScannerData[] scannerData, ScannerFile scannerFile)
        {
            int count = 0;

            foreach (var item in productions)
            {
                var insertResult = await _unicontaAPIService.Insert(item);
                if (insertResult != ErrorCodes.Succes)
                {
                    scannerData[count].Status = $"Error {insertResult}";
                    scannerFile.Status = "Error";
                    // dont need to wait on the errorhandler here it will only slow down the process of creating the production
                    // the task will run when there are available worker threads
                    var task = _errorHandler.WriteError(new UnicontaException("Error while trying to insert Production"), insertResult).ConfigureAwait(false);
                    // It cant set the productionnumber if the production was'nt created therefor it continues
                    continue;
                }

                // Set the given productionnumber on the scannerData given by the api. 
                scannerData[count].ProductionNumber = item.ProductionNumber.ToString();
            }

            return ErrorCodes.Succes;
        }

        /// <summary>
        /// Creates production lines for all productions given in the list <see cref="List{ProductionOrderClient}<"/>
        /// </summary>
        /// <param name="api"></param>
        /// <param name="productions"></param>
        /// <param name="scannerData"></param>
        /// <param name="scannerFile"></param>
        /// <returns></returns>
        public async Task CreateProductionLines(ProductionAPI api, List<ProductionOrderClient> productions, ScannerData[] scannerData, ScannerFile scannerFile)
        {
            int count = 0;
            // Loops through the list of productions to create the needed lines for it
            foreach (ProductionOrderClient production in productions)
            {
                // Uses the ProductionAPI to create the productionlines 
                var creationResult = await api.CreateProductionLines(production, StorageRegister.Register);
                if (creationResult != ErrorCodes.Succes)
                {
                    scannerData[count].Status = $"Error: {creationResult}";
                    scannerFile.Status = "Error";
                    var task = _errorHandler.WriteError(new UnicontaException("Error while creating productionlines", new UnicontaException($"Failed to create lines for production: {production.ProductionNumber}")), creationResult);
                }

                count++;
            }
        }

        /// <summary>
        /// Report all productions as finished
        /// </summary>
        /// <param name="api"></param>
        /// <param name="productions"></param>
        /// <param name="scannerFile"></param>
        /// <param name="scannerData"></param>
        /// <returns></returns>
        public async Task<ErrorCodes> ReportAsFinished(ProductionAPI api, List<ProductionOrderClient> productions, ScannerFile scannerFile, ScannerData[] scannerData)
        {
            ErrorCodes lastError = ErrorCodes.Succes;
            foreach (ProductionOrderClient production in productions)
            {
                // try to find a matching productionnumber in the scannerdata
                var relatedScannerData = scannerData.FirstOrDefault(x => x.ProductionNumber == production.ProductionNumber.ToString());
                if (relatedScannerData == null)
                {
                    // If there is no ScannerData, it will write to log and continue with the next production in the list
                    await _errorHandler.WriteError(new UnicontaException("Error while reporting as finished", new Exception($"Errormessage: There was no ScannerData for production {production.ProductionNumber}")));
                    continue;
                }
                try
                {
                    // try to post the production as finished so it can update the warehouse stock
                    var result = await api.ReportAsFinished(production, DateTime.Now, "", "", "", 0, false, null, production.NoLines, "NR");
                    // If there was an error the journal wont be create therefor the id is 0 and the Err will not have Success
                    if (result.Err != ErrorCodes.Succes && result.JournalPostedlId == 0)
                    {
                        lastError = result.Err;
                        relatedScannerData.Status = $"Error: {result.Err}";
                        scannerFile.Status = "Error";
                        var task = _errorHandler.WriteError(new UnicontaException("Error while reporting as finished", new UnicontaException($"Failed post production: {production.ProductionNumber}")), result.Err);
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    await _errorHandler.WriteError(new UnicontaException("Exception while reporting as finished", new Exception("Errormessage: " + ex.Message)));
                }
            }

            return lastError;
        }
    }
}
