using System;
using RabbitMQ.Client.Events;

namespace EasyNetQ
{
    public class NoOpConsumerErrorStrategy : IConsumerErrorStrategy
    {
        private readonly IEasyNetQLogger _logger;

        public NoOpConsumerErrorStrategy(IEasyNetQLogger logger)
        {
            _logger = logger;
        }

        public void Dispose()
        {
        }

        public void HandleConsumerError(BasicDeliverEventArgs deliverArgs, Exception exception)
        {
            _logger.InfoWrite("No further handling is done for message '{0}' in exchange '{1}' on routing key {2} which failed due to an exception!{3}{4}", deliverArgs.ConsumerTag, deliverArgs.Exchange, deliverArgs.RoutingKey, Environment.NewLine, exception);
        }
    }
}