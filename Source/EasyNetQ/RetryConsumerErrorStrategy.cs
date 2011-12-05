using System;
using System.Collections;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace EasyNetQ
{
    internal static class ExtendsTaskFactory
    {
        public static Task RunDelayed(this TaskFactory value, TimeSpan delay, Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }
            if (delay.TotalMilliseconds < 0)
            {
                throw new ArgumentOutOfRangeException("delay");
            }
            var taskCompletionSource = new TaskCompletionSource<object>();
            var timer = new Timer(self =>
            {
                ((Timer)self).Dispose();
                try
                {
                    action();
                    taskCompletionSource.SetResult(new object());
                }
                catch (Exception exception)
                {
                    taskCompletionSource.SetException(exception);
                }
            });
            timer.Change(delay, delay);
            return taskCompletionSource.Task;
        }
    }

    public static class ExtendsBasicProperties
    {
        private const string __retryCount = "RetryCount";

        public static int GetRetryCount(this IBasicProperties value)
        {
            int result = 0;

            if(value ==null)
                throw new ArgumentNullException("value");

            if (value.Headers != null)
            {
                if (value.Headers.ContainsKey(__retryCount))
                {
                    result = (int)value.Headers[__retryCount];
                }
            }

            return result;
        }

        public static void SetRetryCount(this IBasicProperties value, int numberOfRetries)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            if (value.Headers == null)
            {
                value.Headers = new Hashtable();
            }

            if (!value.Headers.ContainsKey(__retryCount))
            {
                value.Headers.Add(__retryCount, 0);
            }

            value.Headers[__retryCount] = numberOfRetries;
        }

        private static bool ContainsKey(this IDictionary value, object keyToSearchFor)
        {
            if(value == null)
                throw new ArgumentNullException("value");

            if(value.Count == 0)
                return false;

            return value.Keys.Cast<object>().Any(key => key != null && key.Equals(keyToSearchFor));
        }
    }

    public class RetryConsumerErrorStrategy : IConsumerErrorStrategy
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly IConsumerErrorStrategy _fallbackStrategy;
        private readonly IEasyNetQLogger _logger;
        private IConnection _connection;
        private bool _disposed;
        private readonly ThreadLocal<IModel> _threadLocalModel = new ThreadLocal<IModel>();

        public RetryConsumerErrorStrategy(
            IConnectionFactory connectionFactory,
            IEasyNetQLogger logger) :
            this(connectionFactory,
             new NoOpConsumerErrorStrategy(logger),
             logger)
        {

        }

        public RetryConsumerErrorStrategy(
            IConnectionFactory connectionFactory,
            IConsumerErrorStrategy fallbackStrategy,
            IEasyNetQLogger logger
            )
        {
            _connectionFactory = connectionFactory;
            _fallbackStrategy = fallbackStrategy;
            _logger = logger;
            MaxNumberOfRetries = 3;
        }

        public int MaxNumberOfRetries { get; set; }

        private void Connect()
        {
            if (_connection == null || !_connection.IsOpen)
            {
                _connection = _connectionFactory.CreateConnection();
            }
        }

        public void Dispose()
        {
            if (_disposed) return;

            if (_connection != null) _connection.Dispose();
            if(_threadLocalModel.IsValueCreated) _threadLocalModel.Value.Dispose();

            _disposed = true;
        }

        private void DelayHandleError(BasicDeliverEventArgs deliverArgs, Exception exception)
        {
            try
            {
                Connect();

                if (!_threadLocalModel.IsValueCreated)
                {
                    _threadLocalModel.Value = _connection.CreateModel();
                }
                var originalExchange = deliverArgs.Exchange;
                var originalRoutingKey = deliverArgs.RoutingKey;
                var originalMessageBody = deliverArgs.Body;
                var originalCorrelationId = deliverArgs.BasicProperties.CorrelationId;
                var originalType = deliverArgs.BasicProperties.Type;

                int previousNumberOfRetries = deliverArgs.BasicProperties.GetRetryCount();

                var properties = _threadLocalModel.Value.CreateBasicProperties();
                properties.SetPersistent(true);
                properties.Type = originalType;
                properties.CorrelationId = originalCorrelationId;

                if (previousNumberOfRetries < MaxNumberOfRetries)
                {
                    properties.SetRetryCount(previousNumberOfRetries + 1);
                    _threadLocalModel.Value.BasicPublish(originalExchange, originalRoutingKey, properties, originalMessageBody);
                }
                else
                {
                    _fallbackStrategy.HandleConsumerError(deliverArgs, exception);
                }
            }
            catch (BrokerUnreachableException)
            {
                // thrown if the broker is unreachable during initial creation.
                _logger.ErrorWrite("EasyNetQ Consumer Error Handler cannot connect to Broker\n" +
                    CreateConnectionCheckMessage());
            }
            catch (OperationInterruptedException interruptedException)
            {
                // thrown if the broker connection is broken during declare or publish.
                _logger.ErrorWrite("EasyNetQ Consumer Error Handler: Broker connection was closed while attempting to publish Error message.\n" +
                    string.Format("Message was: '{0}'\n", interruptedException.Message) +
                    CreateConnectionCheckMessage());
            }
            catch (Exception unexpecctedException)
            {
                // Something else unexpected has gone wrong :(
                _logger.ErrorWrite("EasyNetQ Consumer Error Handler: Failed to publish error message\nException is:\n"
                    + unexpecctedException);
            }
        }

        public void HandleConsumerError(BasicDeliverEventArgs deliverArgs, Exception exception)
        {
            Task.Factory.RunDelayed(TimeSpan.FromSeconds(3), () => DelayHandleError(deliverArgs, exception));
        }

        private string CreateConnectionCheckMessage()
        {
            return "Failed to write retry message to error queue";
        }
    }
}
