using Alaska.Library.Models.Uniconta.Userdefined;
using FolderWatchService.Core.Handlers;
using FolderWatchService.Services;
using FromXSDFile.OIOUBL.ExportImport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uniconta.API.Inventory;
using Uniconta.API.Service;
using Uniconta.API.System;
using Uniconta.ClientTools;
using Uniconta.ClientTools.DataModel;
using Uniconta.Common;
using Uniconta.DataModel;

namespace FolderWatchService.Core.Managers
{
    public class ProductionManager
    {
        private readonly IUnicontaAPIService _unicontaAPIService;

        public ProductionManager(IUnicontaAPIService unicontaAPIService)
        {
            _unicontaAPIService = unicontaAPIService;
        }

        public async Task<ErrorCodes> HandleCreateProduction(CrudAPI api, string fileName)
        {
            // Creates a query filter
            var queryFilter = new List<PropValuePair>() { PropValuePair.GenereteWhereElements("KeyName", fileName, CompareOperator.Equal) };
            // Query with filter on filename 
            var scannerFile = (await api.Query<ScannerFile>(queryFilter)).FirstOrDefault();

            if (scannerFile == null)
            {
                return ErrorCodes.NoLinesFound;
            }

            var scannerData = await api.Query<ScannerData>(scannerFile);
            var Items = await api.Query<InvItemClient>();
            var productionGroups = await api.Query<ProductionOrderGroupClient>();
            var productionGroup = productionGroups.FirstOrDefault(x => x.KeyName == "ScannerImport");
            List<ProductionOrderClient> productions = new List<ProductionOrderClient>();

            foreach (ScannerData data in scannerData)
            {
                var item = Items.FirstOrDefault(x => x.Item == data.ItemNumber).Item;
                data.Status = "Initiated";
                data.Validated = true;
                var production = GenerateProduction(api, productions, data, item, productionGroup);
                productions.Add(production);
            }

            if (productions.Count > 0)
            {
                var insertResult = await api.Insert(productions);

                if (insertResult != ErrorCodes.Succes)
                {
                    await CreateProductions(api, productions, scannerData, scannerFile);
                }
                else
                    for (int i = 0; i < productions.Count(); i++)
                    {
                        scannerData[i].ProductionNumber = productions[i].ProductionNumber.ToString();
                    }
            }

            var prodApi = new ProductionAPI(api);

            await CreateProductionLines(prodApi, productions, scannerData, scannerFile);

            var postResult = await ReportAsFinished(prodApi, productions, scannerFile, scannerData);

            if(postResult != ErrorCodes.Succes)


            if (string.IsNullOrEmpty(scannerFile.Status))
                scannerFile.Status = "Afsluttet";

            var updateResult = await api.Update(scannerFile);

            if (updateResult != ErrorCodes.Succes)
            {
                await ErrorHandler.WriteError(new UnicontaException("Error while updating ScannerFile"), updateResult).ConfigureAwait(false);
                return updateResult;
            }

            updateResult = await api.Update(scannerData);

            if (updateResult != ErrorCodes.Succes)
            {
                await ErrorHandler.WriteError(new UnicontaException("Error while updating ScannerData"), updateResult).ConfigureAwait(false);
                return updateResult;
            }

            return ErrorCodes.Succes;
        }
        private ProductionOrderClient GenerateProduction(CrudAPI api, List<ProductionOrderClient> productions, ScannerData scannerData, string prodItem, ProductionOrderGroupClient productionOrderGroupClient)
        {
            ProductionOrderClient productionOrder = new ProductionOrderClient();

            productionOrder.SetMaster(scannerData);
            productionOrder.ProdItem = prodItem;
            productionOrder.ProdQty = scannerData.Quantity;
            productionOrder.Group = productionOrderGroupClient.KeyStr;
            productionOrder.Storage = "Ordered";

            return productionOrder;
        }

        private async Task<ErrorCodes> CreateProductions(CrudAPI api, List<ProductionOrderClient> productions, ScannerData[] scannerData, ScannerFile scannerFile)
        {
            int count = 0;

            foreach (var item in productions)
            {
                var insertResult = await api.Insert(item);
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

        private async Task CreateProductionLines(ProductionAPI api, List<ProductionOrderClient> productions, ScannerData[] scannerData, ScannerFile scannerFile)
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

        private async Task<ErrorCodes> ReportAsFinished(ProductionAPI api, List<ProductionOrderClient> productions, ScannerFile scannerFile, ScannerData[] scannerData)
        {
            ErrorCodes lastError = ErrorCodes.Succes;
            foreach (ProductionOrderClient production in productions)
            {
                var relatedScannerData = scannerData.FirstOrDefault(x => x.ProductionNumber == production.ProductionNumber.ToString());
                try
                {
                    var result = await api.ReportAsFinished(production, DateTime.Now, "", "", "", 0, false, null, production.NoLines, "NR");
                   
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
