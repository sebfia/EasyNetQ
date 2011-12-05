using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using EasyNetQ.Config;
using RabbitMQ.Client;

namespace EasyNetQ.Host
{
    public class GenericService
    {
        private IBus _bus;
        private readonly Lazy<IConfigureThisEndpoint> _endpoint;
        private readonly ILog _logger;
        private readonly List<IWantToRunAtStartup> _startupRunners;

        public GenericService(Lazy<IConfigureThisEndpoint> endpoint)
        {
            _endpoint = endpoint;
            _startupRunners = new List<IWantToRunAtStartup>();
            _logger = LogManager.GetCurrentClassLogger();
        }

        public void Start()
        {
            _logger.Debug(m => m("Checking if endpoint implements IWantCustomInitialization"));

            string endpointName = null;

            if (_endpoint.Value is IWantCustomInitialization)
            {
                try
                {
                    _logger.Debug(m=>m("Initializing: ", _endpoint.Value.GetType().Name));
                    (_endpoint.Value as IWantCustomInitialization).Initialize();
                }
                catch (Exception exception)
                {
                    _logger.Error(m => m("Failed to initialize: '" + _endpoint.Value.GetType().Name + "' with error: " + exception.Message));
                }
            }

            if (_endpoint.Value is IHaveEndpointName)
            {
                endpointName = (_endpoint.Value as IHaveEndpointName).EndpointName;
            }

            _logger.Info(m => m("Configuring the bus."));

            ConnectionStringSettings connectionString = null;
            Configuration configuration = null;
            TimeSpan? connectTimeout = null;

            if (_endpoint.Value is IHaveConnectionProperties)//if the endpoint has connection properties we use them
            {
                _logger.Debug(m => m("Using the enpoint's connection properties."));

                var connectionProperties = _endpoint.Value as IHaveConnectionProperties;

                Configure.With().ConnectionFactory(new ConnectionFactoryWrapper(
                                                       new ConnectionFactory
                                                           {
                                                               HostName =
                                                                   String.IsNullOrWhiteSpace(connectionProperties.Host)
                                                                       ? EasyNetQConnection.DefaultConnection.Host
                                                                       : connectionProperties.Host,
                                                               VirtualHost =
                                                                   String.IsNullOrWhiteSpace(
                                                                       connectionProperties.VirtualHost)
                                                                       ? EasyNetQConnection.DefaultConnection.VirtualHost
                                                                       : connectionProperties.VirtualHost,
                                                               UserName =
                                                                   String.IsNullOrWhiteSpace(
                                                                       connectionProperties.Username)
                                                                       ? EasyNetQConnection.DefaultConnection.Username
                                                                       : connectionProperties.Username,
                                                               Password = connectionProperties.Password ?? EasyNetQConnection.DefaultConnection.Password
                                                           }));
            }
            else if((connectionString = ConfigurationManager.ConnectionStrings["rabbit"]) != null)//if the configuration has a connection string named 'rabbit' we use this one
            {
                _logger.Debug(m => m("Using the 'rabbit' connection string in app settings"));

                var connectionValues = new ConnectionString(connectionString.ConnectionString);

                Configure.With().ConnectionFactory(new ConnectionFactoryWrapper(
                                                       new ConnectionFactory
                                                           {
                                                               HostName = connectionValues.Host,
                                                               VirtualHost = connectionValues.VirtualHost,
                                                               UserName = connectionValues.UserName,
                                                               Password = connectionValues.Password
                                                           }));
            }
            else if ((configuration = _endpoint.Value.GetType().OpenMappedEnpointConfiguration()) != null)
                //if our configuration section contains connection elements..
            {
                try
                {
                    EasyNetQConfigurationSection section = configuration.GetEasyNetQSection();

                    if (section != null)
                    {
                        _logger.Debug(m => m("Using connection properties from EasyNetQ configuration section"));

                        connectTimeout = TimeSpan.FromSeconds(section.Connection.Timeout);
                        //only set endpoint name if it has not been set by the endpoint-config
                        endpointName = String.IsNullOrWhiteSpace(endpointName) ? section.Endpoint.Name : endpointName;

                        Configure.With().ConnectionFactory(new ConnectionFactoryWrapper(
                                                               new ConnectionFactory
                                                               {
                                                                   HostName = section.Connection.Host,
                                                                   VirtualHost = section.Connection.VirtualHost,
                                                                   UserName = section.Connection.Username,
                                                                   Password = section.Connection.Password
                                                               }));
                    }
                }
                catch (Exception exception)
                {
                    _logger.Error(m => m("Unable to open EasyNetQConfigurationSection due to error: {0}", exception.Message));
                    _logger.Debug(m => m(exception.ToString()));
                    throw;
                }
            }
            else //now we use a default bus on localhost
                _logger.Debug(m => m("Using the default bus connection"));

            if (String.IsNullOrWhiteSpace(endpointName))
            {
                endpointName = String.Format("{0}_v{1}", _endpoint.Value.GetType().AssemblyQualifiedName,
                                             _endpoint.Value.GetType().Assembly.GetName().Version);
            }

            _logger.Info(m => m("Configuring endpoint with name '{0}'", endpointName));

            Configure.With().EndpointName(endpointName);

            connectTimeout = connectTimeout.HasValue ? connectTimeout.Value : TimeSpan.FromSeconds(10);

            _logger.Debug(m => m("Connecting to bus with timeout of {0} seconds", connectTimeout.Value.TotalSeconds));

            try
            {
                var timeout = DateTime.Now.Add(connectTimeout.Value);

                _bus = Configure.Instance.BusBuilder.CreateBus();

                while (!_bus.IsConnected)
                {
                    if (DateTime.Now.IsLaterThan(timeout))
                    {
                        throw new TimeoutException("Connecting to bus timed out!");
                    }

                    Thread.Sleep(10);
                }

                _logger.Info(m => m("Connected to bus at time {0}", DateTime.Now.ToShortTimeString()));
            }
            catch (Exception exception)
            {
                _logger.Fatal(m => m("Error creating bus!{0}{1}{0}{2}", Environment.NewLine, exception.Message, exception.ToString()));
                throw;
            }
            
            Configure.Instance.ConfigureComponents.RegisterSingleton(typeof (IBus), _bus);

            var startupRunners = Configure.Instance.Builder.BuildAll<IWantToRunAtStartup>();

            if (startupRunners != null)
            {
                foreach (IWantToRunAtStartup startupRunner in startupRunners)
                {
                    _startupRunners.Add(startupRunner);
                    IWantToRunAtStartup runner = startupRunner;
                    Task.Factory.StartNew(() =>
                                              {
                                                  var logger = LogManager.GetLogger(runner.GetType());

                                                  try
                                                  {
                                                      logger.Debug(m => m("Calling start method on {0}", 
                                                          runner.GetType().Name));
                                                      runner.Start();
                                                  }
                                                  catch (Exception exception)
                                                  {
                                                      logger.Error(m => m("A {0} was thrown calling start method on {1}{2}{3}{2}{4}", 
                                                          exception.GetType().Name, 
                                                          runner.GetType().Name, 
                                                          Environment.NewLine, 
                                                          exception.Message, 
                                                          exception.ToString()));
                                                  }
                                              });
                }
            }
        }

        public void Pause()
        {
            _logger.Info(m => m("Received pause signal."));
        }

        public void Continue()
        {
            _logger.Info(m => m("Received continue signal."));
        }

        public void Stop()
        {
            _logger.Info(m => m("Received stop signal."));

            _logger.Debug(m => m("Stopping startup runners."));
            foreach (var startupRunner in _startupRunners)
            {
                startupRunner.Stop();
            }

            _logger.Debug(m => m("Disposing object builder."));
            Configure.Instance.Builder.Dispose();
        }
    }
}