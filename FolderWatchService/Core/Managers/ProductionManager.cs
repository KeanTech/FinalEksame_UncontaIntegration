using Alaska.Library.Models.Uniconta.Userdefined;
using FolderWatchService.Core.Handlers;
using FolderWatchService.Services;
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
                data.Status = "Afsluttet";
                data.Validated = true;
                var production = GenerateProduction(api, productions, data, item, productionGroup);
                productions.Add(production);
            }

            if (productions.Count > 0)
            {
                var insertResult = await api.Insert(productions);
                if (insertResult != ErrorCodes.Succes)
                {
                    await CreateProductions(api, productions, scannerData);
                }
            }

            var prodApi = new ProductionAPI(api);
            
            var task = CreateProductionLines(prodApi, productions).ConfigureAwait(false);

            scannerFile.Status = "Afsluttet";
            scannerFile.Production = "";
            productions.ForEach(x => { scannerFile.Production += x.ProductionNumber + " "; });
            
            var updateResult = await api.Update(scannerFile);

            if (updateResult != ErrorCodes.Succes)
            {
                task = ErrorHandler.WriteError(new UnicontaException("Error while updating ScannerFile"), updateResult).ConfigureAwait(false);
                return updateResult;
            }

            updateResult = await api.Update(scannerData);

            if (updateResult != ErrorCodes.Succes)
            {
                task = ErrorHandler.WriteError(new UnicontaException("Error while updating ScannerData"), updateResult).ConfigureAwait(false);
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

        private async Task<ErrorCodes> CreateProductions(CrudAPI api, List<ProductionOrderClient> productions, ScannerData[] scannerData)
        {
            bool updateScannerData = false;
            int count = 0;

            foreach (var item in productions)
            {
                var insertResult = await api.Insert(item);
                if (insertResult != ErrorCodes.Succes)
                {
                    scannerData[count].Status = $"Api error {insertResult}";
                    var task = ErrorHandler.WriteError(new UnicontaException("Error while trying to insert Production"), insertResult).ConfigureAwait(false);
                    updateScannerData = true;
                }
            }

            return ErrorCodes.Succes;
        }

        private async Task CreateProductionLines(ProductionAPI api, List<ProductionOrderClient> productions)
        {
            foreach (ProductionOrderClient production in productions)
            {
                var creationResult = await api.CreateProductionLines(production, StorageRegister.Register);
                if (creationResult != ErrorCodes.Succes)
                {
                    var task = ErrorHandler.WriteError(new UnicontaException("Error while creating productionlines", new UnicontaException($"Failed to create lines for production: {production.ProductionNumber}")), creationResult);
                }
            }
        }

    }
}
