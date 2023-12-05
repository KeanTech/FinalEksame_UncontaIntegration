using Alaska.Library.Core.Factories;
using Autofac;
using FolderWatchService.Core.Generators;
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
    // This class is used to configure the servce container
    public static class AppServiceContainer
    {
        // This method is static so that it does'nt have to be instantiated in the program to get the container.
        public static IContainer Configure() 
        {
            ContainerBuilder builder = new ContainerBuilder();
            // Here all dependencies is defined
            builder.RegisterType<ErrorHandler>().As<IErrorHandler>();
            builder.RegisterType<Factory>().As<IFactory<IEntity>>();
            builder.RegisterType<UnicontaFactory>().As<IUnicontaFactory>();
            builder.RegisterType<KeyGenerator>().AsSelf().SingleInstance();
            builder.RegisterType<EncryptionManager>();
            builder.RegisterType<ConfigManager>().As<IConfigManager>();
            // Register as a singleton
            builder.RegisterType<UnicontaAPIService>().As<IUnicontaAPIService>().SingleInstance();
            builder.RegisterType<ProductionService>().As<IProductionService>();
            builder.RegisterType<ProductionManager>().As<IProductionManager>();

            return builder.Build();
        }
    }
}
