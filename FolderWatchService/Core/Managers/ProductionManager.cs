using Alaska.Library.Core.Factories;
using Alaska.Library.Models.Uniconta.Userdefined;
using FolderWatchService.Core.Handlers;
using FolderWatchService.Services;
using FromXSDFile.OIOUBL.ExportImport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Controls;
using Uniconta.API.Inventory;
using Uniconta.API.Service;
using Uniconta.API.System;
using Uniconta.ClientTools;
using Uniconta.ClientTools.DataModel;
using Uniconta.Common;
using Uniconta.DataModel;

namespace FolderWatchService.Core.Managers
{
    public class ProductionManager : IProductionManager
    {
        private readonly IUnicontaAPIService _unicontaAPIService;
        private readonly IUnicontaFactory _factory;
        private readonly IProductionService _productionService;
        private const string _productionGroupName = "ScannerImport";
        public ProductionManager(IUnicontaAPIService unicontaAPIService, IUnicontaFactory factory, IProductionService productionService)
        {
            _unicontaAPIService = unicontaAPIService;
            _factory = factory;
            _productionService = productionService;
        }

        public async Task<ErrorCodes> HandleCreateProduction(string fileName)
        {
            ErrorCodes apiResult = ErrorCodes.Succes;
            // Creates a query filter
            var queryFilter = new List<PropValuePair>() { PropValuePair.GenereteWhereElements("KeyName", fileName, CompareOperator.Equal) };
            // Query with filter on filename 
            var scannerFile = (await _unicontaAPIService.Query<ScannerFile>(queryFilter)).FirstOrDefault();

            if (scannerFile == null)
            {
                return ErrorCodes.NoLinesFound;
            }

            var scannerData = await _unicontaAPIService.Query<ScannerData>(scannerFile);
            var Items = await _unicontaAPIService.Query<InvItemClient>();
            var productionGroups = await _unicontaAPIService.Query<ProductionOrderGroupClient>();
            var productionGroup = productionGroups.FirstOrDefault(x => x.KeyName == _productionGroupName);
            if (productionGroup == null)
            {
                ProductionOrderGroupClient groupClient = _factory.Create<ProductionOrderGroupClient>();
                groupClient.KeyStr = "Import From Scanner";
                groupClient.Name = _productionGroupName;
                apiResult = await _unicontaAPIService.Insert(groupClient);
            }

            List<ProductionOrderClient> productions = _factory.CreateListOf<ProductionOrderClient>();

            // creates a new production foreach line in the scannerData array
            foreach (ScannerData data in scannerData)
            {
                // 
                var item = Items.FirstOrDefault(x => x.Item == data.ItemNumber).Item;
                data.Status = "Initiated";
                data.Validated = true;
                var production = GenerateProduction(data, item, productionGroup);
                productions.Add(production);
            }

            if (productions.Count > 0)
            {
                // Using the unicontaAPIService to create the productions in Uniconta
                apiResult = await _unicontaAPIService.Insert(productions);

                if (apiResult != ErrorCodes.Succes)
                {
                    await _productionService.CreateProductions(productions, scannerData, scannerFile);
                }
                else
                    for (int i = 0; i < productions.Count(); i++)
                    {
                        scannerData[i].ProductionNumber = productions[i].ProductionNumber.ToString();
                    }
            }

            var prodApi = _unicontaAPIService.CreateProductionApi();

            await _productionService.CreateProductionLines(prodApi, productions, scannerData, scannerFile);

            apiResult = await _productionService.ReportAsFinished(prodApi, productions, scannerFile, scannerData);

            if (apiResult != ErrorCodes.Succes)


                if (string.IsNullOrEmpty(scannerFile.Status) || scannerFile.Status == "Initiated")
                    scannerFile.Status = "Afsluttet";


            var updateResult = await _unicontaAPIService.Update(scannerFile);

            if (updateResult != ErrorCodes.Succes)
            {
                await ErrorHandler.WriteError(new UnicontaException("Error while updating ScannerFile"), updateResult).ConfigureAwait(false);
                return updateResult;
            }

            updateResult = await _unicontaAPIService.Update(scannerData);

            if (updateResult != ErrorCodes.Succes)
            {
                await ErrorHandler.WriteError(new UnicontaException("Error while updating ScannerData"), updateResult).ConfigureAwait(false);
                return updateResult;
            }

            return ErrorCodes.Succes;
        }

        private ProductionOrderClient GenerateProduction(ScannerData scannerData, string prodItem, ProductionOrderGroupClient productionOrderGroupClient)
        {
            ProductionOrderClient productionOrder = _factory.Create<ProductionOrderClient>();

            productionOrder.SetMaster(scannerData);
            productionOrder.ProdItem = prodItem;
            productionOrder.ProdQty = scannerData.Quantity;
            productionOrder.Group = productionOrderGroupClient.KeyStr;
            productionOrder.Storage = "Ordered";

            return productionOrder;
        }

    }
}
