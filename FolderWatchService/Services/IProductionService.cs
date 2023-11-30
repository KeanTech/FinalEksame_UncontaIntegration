using Alaska.Library.Models.Uniconta.Userdefined;
using System.Collections.Generic;
using System.Threading.Tasks;
using Uniconta.API.Inventory;
using Uniconta.ClientTools.DataModel;
using Uniconta.Common;

namespace FolderWatchService.Services
{
    public interface IProductionService
    {
        Task CreateProductionLines(ProductionAPI api, List<ProductionOrderClient> productions, ScannerData[] scannerData, ScannerFile scannerFile);
        Task<ErrorCodes> CreateProductions(List<ProductionOrderClient> productions, ScannerData[] scannerData, ScannerFile scannerFile);
        Task<ErrorCodes> ReportAsFinished(ProductionAPI api, List<ProductionOrderClient> productions, ScannerFile scannerFile, ScannerData[] scannerData);
    }
}