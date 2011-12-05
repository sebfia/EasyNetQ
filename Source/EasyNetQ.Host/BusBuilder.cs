using EasyNetQ.Config;
using EasyNetQ.Loggers;
using RabbitMQ.Client;

namespace EasyNetQ
{
    internal class BusBuilder : IBusConfigurer, IBusBuilder
    {
        private IConnectionFactory _connectionFactory;
        private IConsumerErrorStrategy _consumerErrorStrategy;
        private ISerializer _serializer;
        private IEasyNetQLogger _logger;

        public BusBuilder()
        {
            _serializer = new JsonSerializer();
            _logger = new ConsoleLogger();
        }

        IConnectionFactory IBusConfigurer.ConnectionFactory
        {
            get { return _connectionFactory; }
            set { _connectionFactory = value; }
        }

        IConsumerErrorStrategy IBusConfigurer.ConsumerErrorStrategy
        {
            get { return _consumerErrorStrategy; }
            set { _consumerErrorStrategy = value; }
        }

        ISerializer IBusConfigurer.Serializer
        {
            get { return _serializer; }
            set { _serializer = value; }
        }

        IEasyNetQLogger IBusConfigurer.Logger
        {
            get { return _logger; }
            set { _logger = value; }
        }

        IBus IBusBuilder.CreateBus()
        {
            _logger.DebugWrite("Creating bus.");

            if (_connectionFactory == null)
            {
                _logger.DebugWrite("Using default connection factory.");
                _connectionFactory = CreateDefaultConnectionFactory();
            }

            if (_consumerErrorStrategy == null)
            {
                _logger.DebugWrite("Using default consumer error strategy.");
                _consumerErrorStrategy = CreateDefaultConsumerErrorStrategy();
            }

            return new RabbitBus(
                TypeNameSerializer.Serialize,
                _serializer,
                new QueueingConsumerFactory(_logger, _consumerErrorStrategy),
                _connectionFactory,
                _logger,
                CorrelationIdGenerator.GetCorrelationId);
        }

        private IConsumerErrorStrategy CreateDefaultConsumerErrorStrategy()
        {
            var fallbackStrategy = new DefaultConsumerErrorStrategy(_connectionFactory, _serializer, _logger);
            return new RetryConsumerErrorStrategy(_connectionFactory, fallbackStrategy, _logger);
        }

        private IConnectionFactory CreateDefaultConnectionFactory()
        {
            return new ConnectionFactoryWrapper(
                new ConnectionFactory
                    {
                        HostName = EasyNetQConnection.DefaultConnection.Host,
                        VirtualHost = EasyNetQConnection.DefaultConnection.VirtualHost,
                        UserName = EasyNetQConnection.DefaultConnection.Username,
                        Password = EasyNetQConnection.DefaultConnection.Password
                    });
        }
    }
}