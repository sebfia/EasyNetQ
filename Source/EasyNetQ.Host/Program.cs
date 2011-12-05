using System;
using System.Linq;
using Topshelf;

namespace EasyNetQ.Host
{
    internal class Program
    {
        [LoaderOptimization(LoaderOptimization.MultiDomainHost)]
        private static void Main(string[] args)
        {
            var serviceData = new ServiceData("Test", "Test", "A testing service.");

            if (args.RequestInstallOrUninstall())
            {
                try
                {
                    serviceData = ServiceData.FromArguments(args);
                    serviceData.ThrowIfInvalidInContext(args);
                }
                catch (Exception)
                {
                    var oldFore = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("{0}{0}Invalid usage!", Environment.NewLine);
                    Console.WriteLine(
                        "Usage: EasyNetQ.Host install|uninstall -s:[ServiceName:required] -n:[DisplayName:required for install] -d:[Description]{0}{0}",
                        Environment.NewLine);
                    Console.ForegroundColor = oldFore;
                    return;
                }
            }

            Type endpoint = ScanAssemblies.For<IConfigureThisEndpoint>().FirstOrDefault();

            AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", endpoint.GetEndpointConfigurationFilePath());

            try
            {
                HostFactory.Run(host =>
                                    {
                                        host.SetServiceName(serviceData.Name);
                                        host.SetDisplayName(serviceData.DisplayName);
                                        host.SetDescription(serviceData.Description);

                                        host.Service<ShelfHost>(service =>
                                                                         {
                                                                             service.ConstructUsing((name, coordinator) => new ShelfHost(coordinator));
                                                                             service.WhenStarted(s=>s.Start());
                                                                             service.WhenPaused(s => s.Pause());
                                                                             service.WhenContinued(s => s.Continue());
                                                                             service.WhenStopped(s => s.Stop());
                                                                         });
                                    });
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
                Console.ReadKey(false);
            }
        }
    }
}
