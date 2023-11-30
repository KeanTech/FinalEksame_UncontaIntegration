using Alaska.Library.Core.Factories;
using Autofac;
using FolderWatchService.Core.Containers;
using FolderWatchService.Core.Handlers;
using FolderWatchService.Core.Managers;
using FolderWatchService.Services;
using System;
using Topshelf;

namespace FolderWatchService
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Instantiate the service container 
            IContainer container = AppServiceContainer.Configure();
            // Creates a scope to make sure that the registered services get disposed when the Windows Service stops
            using (var scope = container.BeginLifetimeScope())
            { 
                // here we set the configuration for the service host
                var exitCode = HostFactory.Run(x =>
                {
                    // tells topshelf witch service class to run
                    x.Service<FolderService>(s =>
                    {
                        // the construktor for the service
                        s.ConstructUsing(runner =>
                        new FolderService(
                            // adds the needed services to the constructor
                            scope.Resolve<IProductionManager>(),
                            scope.Resolve<IConfigManager>(),
                            scope.Resolve<IUnicontaAPIService>()

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
}
