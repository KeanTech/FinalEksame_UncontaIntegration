using System.Threading.Tasks;
using Uniconta.API.System;
using Uniconta.Common;

namespace FolderWatchService.Core.Managers
{
    public interface IProductionManager
    {
        Task<ErrorCodes> HandleCreateProduction(string fileName, bool reportAsFinished);
    }
}