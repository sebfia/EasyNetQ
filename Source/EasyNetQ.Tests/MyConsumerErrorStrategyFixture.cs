using System;
using System.Threading;
using NUnit.Framework;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing.v0_9_1;
using Rhino.Mocks;

namespace EasyNetQ.Tests
{
    [TestFixture]
    public class MyConsumerErrorStrategyFixture
    {
        private IConsumerErrorStrategy _sut;
        private IConsumerErrorStrategy _fallbackStrategy;
        private IEasyNetQLogger _logger;
        private IConnectionFactory _connectionFactory;
        private IConnection _connection;
        private IModel _model;

        [SetUp]
        public void SetUp()
        {
            _model = MockRepository.GenerateMock<IModel>();
            _model.Stub(x => x.CreateBasicProperties())
                .Return(new BasicProperties());
            _model.Stub(x => x.BasicPublish(null, null, null, null))
                .IgnoreArguments();
            _model.Replay();

            _connection = MockRepository.GenerateMock<IConnection>();
            _connection.Stub(x => x.CreateModel()).Return(_model);
            _connection.Replay();

            _connectionFactory = MockRepository.GenerateMock<IConnectionFactory>();
            _connectionFactory.Stub(x => x.CreateConnection()).Return(_connection);
            _connectionFactory.Replay();

            _logger = MockRepository.GenerateMock<IEasyNetQLogger>();
            _logger.Replay();

            _fallbackStrategy = MockRepository.GenerateMock<IConsumerErrorStrategy>();
            _fallbackStrategy.Stub(x => x.HandleConsumerError(null, null))
                .IgnoreArguments();
            _fallbackStrategy.Replay();

            _sut = new RetryConsumerErrorStrategy(
                _connectionFactory,
                _fallbackStrategy,
                _logger);
        }

        [Test]
        public void FirstMessageHandled()
        {
            var basicDeliverEventArgs = new BasicDeliverEventArgs(Guid.NewGuid().ToString(), 0, false, 
                "Test", "Test", new BasicProperties(), new byte[16]);
            _sut.HandleConsumerError(basicDeliverEventArgs, new Exception("Test"));

            Thread.Sleep(TimeSpan.FromSeconds(4));

            _model.AssertWasCalled(x => x.BasicPublish(Arg<string>.Is.Equal("Test"), Arg<string>.Is.Equal("Test"), Arg<IBasicProperties>.Matches(z=>z.GetRetryCount() == 1), Arg<byte[]>.Is.Anything));

            _logger.AssertWasNotCalled(x => x.ErrorWrite(null, null), y => y.IgnoreArguments());
        }

        [Test]
        public void HandleSecondErroredMessage()
        {
            var basicProperties = new BasicProperties();
            basicProperties.SetRetryCount(2);
            var basicDeliverEventArgs = new BasicDeliverEventArgs(Guid.NewGuid().ToString(), 0, false,
                                              "Test", "Test", basicProperties, new byte[16]);

            _sut.HandleConsumerError(basicDeliverEventArgs, new Exception("Test"));

            Thread.Sleep(TimeSpan.FromSeconds(4));

            _model.AssertWasCalled(x => x.BasicPublish(Arg<string>.Is.Equal("Test"), 
                Arg<string>.Is.Equal("Test"), 
                Arg<IBasicProperties>.Matches(z => z.GetRetryCount() == 3), 
                Arg<byte[]>.Is.Anything));

            _logger.AssertWasNotCalled(x => x.ErrorWrite(null, null), y => y.IgnoreArguments());
        }

        [Test]
        public void HandlingLastErrorMessageCallsFallback()
        {
            var basicProperties = new BasicProperties();
            basicProperties.SetRetryCount(3);
            var basicDeliverEventArgs = new BasicDeliverEventArgs(Guid.NewGuid().ToString(), 0, false,
                                              "Test", "Test", basicProperties, new byte[16]);

            _sut.HandleConsumerError(basicDeliverEventArgs, new Exception("Test"));

            Thread.Sleep(TimeSpan.FromSeconds(4));

            _model.AssertWasNotCalled(x => x.BasicPublish(null, null, null, null), y => y.IgnoreArguments());
            _fallbackStrategy.AssertWasCalled(x => x.HandleConsumerError(null, null), y => y.IgnoreArguments()
                .Message("Expected invocation of fallback strategy!"));

            _logger.AssertWasNotCalled(x => x.ErrorWrite(null, null), y => y.IgnoreArguments()
                .Message("RetryMessageConsumerErrorStrategy should not be called any more at this point!"));
        }   
    }
}
