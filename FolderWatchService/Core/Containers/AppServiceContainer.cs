using Alaska.Library.Core.Factories;
using Autofac;
using FolderWatchService.Core.Handlers;
using FolderWatchService.Core.Managers;
using FolderWatchService.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderWatchService.Core.Containers
{
    public static class AppServiceContainer
    {
        public static IContainer Configure() 
        {
            ContainerBuilder builder = new ContainerBuilder();

            builder.RegisterType<ErrorHandler>();
            builder.RegisterType<Factory>().As<IFactory>();
            builder.RegisterType<EncryptionManager>();
            builder.RegisterType<ConfigManager>().As<IConfigManager>();
            builder.RegisterType<ProductionManager>();
            builder.RegisterType<UnicontaAPIService>().As<IUnicontaAPIService>();

            return builder.Build();
        }
    }
}
