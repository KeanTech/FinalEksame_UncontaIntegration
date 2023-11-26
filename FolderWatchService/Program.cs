using Alaska.Library.Core.Factories;
using Autofac;
using FolderWatchService.Core.Handlers;
using FolderWatchService.Core.Managers;
using FolderWatchService.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Topshelf;
using Uniconta.ClientTools.DataModel;

namespace FolderWatchService
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Uses Autofac to register all dependencies/service
            var builder = new ContainerBuilder();
            builder.RegisterType<UnicontaAPIService>().As<IUnicontaAPIService>();
            builder.RegisterType<ErrorHandler>().AsSelf();
            builder.RegisterType<EncryptionManager>().AsSelf();
            builder.RegisterType<ConfigManager>().As<IConfigManager>(); 
            builder.RegisterType<Factory>().As<IFactory>();

            var serviceContainer = builder.Build();
            // here we set the configuration for the service host
            var exitCode = HostFactory.Run(x =>
            {
               // tells topshelf witch service class to run
                x.Service<FolderService>(s =>
                {
                    // the construktor for the service
                    s.ConstructUsing(runner => 
                    new FolderService(
                        
                        serviceContainer.Resolve<ErrorHandler>(),
                        serviceContainer.Resolve<IConfigManager>()
                        
                        ));
                    // Sets the method to call on start
                    s.WhenStarted(runner => runner.Start());
                    // Sets the method to call on stop
                    s.WhenStopped(runner => runner.Stop());
                });
                // Set the service to be local
                x.RunAsLocalService();
                // Name of the service
                x.SetServiceName("Alaska.FolderWatchService");
                // Display name on the service
                x.SetDisplayName("Alaska.FolderWatchService");
                // Description on the service
                x.SetDescription("This is the folder watch that is you to import scanner files to Uniconta");
                // Here it uses manually start so its not startet if the computer turns off
                x.StartManually();

            });

            // gets the exit code from the service if the service cannot start or it terminates because of an error
            int exitCodeValue = (int)Convert.ChangeType(exitCode, exitCode.GetTypeCode());
            // Tell the operatingsystem witch code the service terninated with
            Environment.Exit(exitCodeValue);
        }
    }
}
