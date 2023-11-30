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

        public ProductionService(IUnicontaAPIService unicontaAPIService)
        {
            _unicontaAPIService = unicontaAPIService;
        }

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
                    var task = ErrorHandler.WriteError(new UnicontaException("Error while trying to insert Production"), insertResult).ConfigureAwait(false);
                    continue;
                }

                scannerData[count].ProductionNumber = item.ProductionNumber.ToString();
            }

            return ErrorCodes.Succes;
        }

        public async Task CreateProductionLines(ProductionAPI api, List<ProductionOrderClient> productions, ScannerData[] scannerData, ScannerFile scannerFile)
        {
            int count = 0;

            foreach (ProductionOrderClient production in productions)
            {
                var creationResult = await api.CreateProductionLines(production, StorageRegister.Register);
                if (creationResult != ErrorCodes.Succes)
                {
                    scannerData[count].Status = $"Error: {creationResult}";
                    scannerFile.Status = "Error";
                    var task = ErrorHandler.WriteError(new UnicontaException("Error while creating productionlines", new UnicontaException($"Failed to create lines for production: {production.ProductionNumber}")), creationResult);
                }

                count++;
            }
        }

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
                    await ErrorHandler.WriteError(new UnicontaException("Error while reporting as finished", new Exception($"Errormessage: There was no ScannerData for production {production.ProductionNumber}")));
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
                        var task = ErrorHandler.WriteError(new UnicontaException("Error while reporting as finished", new UnicontaException($"Failed post production: {production.ProductionNumber}")), result.Err);
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    await ErrorHandler.WriteError(new UnicontaException("Exception while reporting as finished", new Exception("Errormessage: " + ex.Message)));
                }
            }

            return lastError;
        }
    }
}
