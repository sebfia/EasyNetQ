using System;
using System.Linq;
using Common.Logging;
using Topshelf.Configuration.Dsl;
using Topshelf.Shelving;

namespace EasyNetQ.Host
{
    public class GenericServiceBootstrapper : Bootstrapper<GenericService>
    {
        public void InitializeHostedService(IServiceConfigurator<GenericService> cfg)
        {
            Type endpoint = ScanAssemblies.For<IConfigureThisEndpoint>().FirstOrDefault();
            CheckEndpointNotNullAndHasEmptyConstructor(endpoint);

            cfg.ConstructUsing((d, name, channel)=>
                                   {
                                       ILog log;
                                       
                                       try
                                       {
                                           //Configure.Instance.Logging.Configure(endpoint);
                                           log = LogManager.GetCurrentClassLogger();
                                       }
                                       catch (Exception exception)
                                       {
                                           Console.Out.WriteLine("An exception was thrown while configuring the logging framework.{0}{1}", 
                                               Environment.NewLine, 
                                               exception);
                                           throw;
                                       }

                                       log.Debug(m => m("Logging has been configured!"));
                                       return new GenericService(new Lazy<IConfigureThisEndpoint>(()=>(IConfigureThisEndpoint)Activator.CreateInstance(endpoint)));
                                   });
            cfg.SetServiceName(GenerateEndpointName(endpoint));
            cfg.WhenStarted(s => s.Start());
            cfg.WhenPaused(s => s.Pause());
            cfg.WhenContinued(s => s.Continue());
            cfg.WhenStopped(s=>s.Stop());
        }

        private static void CheckEndpointNotNullAndHasEmptyConstructor(Type endpoint)
        {
            if (endpoint == null)
                throw new InvalidOperationException("No configurable endpoint found!");

            if (endpoint.GetConstructor(Type.EmptyTypes) == null)
            {
                throw new InvalidOperationException("Endpoint configuration type needs to have a default constructor: " + endpoint.FullName);
            }
        }

        private static string GenerateEndpointName(Type endpointType)
        {
            return String.Format("{0}_v{1}", endpointType.AssemblyQualifiedName, endpointType.Assembly.GetName().Version.ToString());
        }
    }
}