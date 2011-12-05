using System;
using NUnit.Framework;
using Rhino.Mocks;

namespace EasyNetQ.Tests
{
    public class MessageHandler : IHandleMessage<TestMessage>
    {
        void IHandleMessage<TestMessage>.Handle(TestMessage message)
        {

        }
    }

    [TestFixture]
    public class ExtendsBusFixture
    {
        [Test]
        public void CheckThatSubscribeIsCalledWhenAddingHandler()
        {
            var bus = MockRepository.GenerateMock<IBus>();
            bus.Replay();

            bus.RegisterHandler(new MessageHandler(), "test");

            bus.AssertWasCalled(x => x.Subscribe(
                Arg<string>.Matches(s => s == "test"),
                Arg<Action<TestMessage>>.Is.Anything),
                                y => y.IgnoreArguments());
        }
    }
}