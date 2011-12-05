using System;

namespace EasyNetQ
{
    public static class ConfigureBus
    {
        public static Configure ConnectionFactory<TConnectionFactory>(this Configure value) where TConnectionFactory:class, IConnectionFactory, new()
        {
            return value.ConnectionFactory((IConnectionFactory) Activator.CreateInstance<TConnectionFactory>());
        }

        public static Configure ConnectionFactory<TConnectionFactory>(this Configure value, TConnectionFactory connectionFactory) where TConnectionFactory : class, IConnectionFactory
        {
            if(value == null)
                throw new ArgumentNullException("value");
            if(connectionFactory == null)
                throw new ArgumentNullException("connectionFactory");

            value.Bus.ConnectionFactory = connectionFactory;

            return value;
        }

        public static Configure ConsumerErrorStrategy<TConsumerErrorStrategy>(this Configure value) where TConsumerErrorStrategy : class, IConsumerErrorStrategy, new()
        {
            return value.ConsumerErrorStrategy(new TConsumerErrorStrategy());
        }

        public static Configure ConsumerErrorStrategy<TConsumerErrorStrategy>(this Configure value, TConsumerErrorStrategy consumerErrorStrategy) where TConsumerErrorStrategy : class, IConsumerErrorStrategy
        {
            if (value == null)
                throw new ArgumentNullException("value");
            if (consumerErrorStrategy == null)
                throw new ArgumentNullException("consumerErrorStrategy");

            value.Bus.ConsumerErrorStrategy = consumerErrorStrategy;

            return value;
        }

        public static Configure Serializer<TSerializer>(this Configure value) where TSerializer : class, ISerializer, new()
        {
            return value.Serializer(new TSerializer());
        }

        public static Configure Serializer<TSerializer>(this Configure value, TSerializer serializer) where TSerializer : class, ISerializer
        {
            if(value == null)
                throw new ArgumentNullException("value");
            if(serializer == null)
                throw new ArgumentNullException("serializer");

            value.Bus.Serializer = serializer;

            return value;
        }

        public static Configure Logger<TLogger>(this Configure value) where TLogger : class, IEasyNetQLogger, new()
        {
            return value.Logger(new TLogger());
        }

        public static Configure Logger<TLogger>(this Configure value, TLogger logger) where TLogger : class, IEasyNetQLogger
        {
            if (value == null)
                throw new ArgumentNullException("value");
            if(logger == null)
                throw new ArgumentNullException("logger");

            value.Bus.Logger = logger;

            return value;
        }
    }
}