using Alaska.Library.Models.Uniconta.Userdefined;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uniconta.API.System;
using Uniconta.Common;
using Uniconta.DataModel;

namespace FolderWatchService.Services
{
    public interface IUnicontaAPIService : IDisposable
    {
        Task<CrudAPI> Login();
        Task<ErrorCodes> Create(ScannerFile scannerFile, List<ScannerData> scannerData);
        Task<ErrorCodes> Update(ScannerFile scannerFile, List<ScannerData> scannerData);
        Task<List<InvItem>> GetInventory();
    }
}
