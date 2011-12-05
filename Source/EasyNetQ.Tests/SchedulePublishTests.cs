using System;
using EasyNetQ.SystemMessages;
using NUnit.Framework;
using Rhino.Mocks;

namespace EasyNetQ.Tests
{
    [TestFixture]
    public class SchedulePublishTests
    {
        private ISchedulingBus _sut;
        private ISerializer _mockSerializer;

        [SetUp]
        public void SetUp()
        {
            new MockModel();
            _mockSerializer = MockRepository.GenerateMock<ISerializer>();
            _mockSerializer.Stub(x => x.MessageToBytes<object>(null))
                .IgnoreArguments();
            _mockSerializer.Replay();

            var factory = new TestBusFactory
                              {
                                  Serializer = _mockSerializer
                              };

            _sut = factory.CreateBusWithMockAmqpClient() as ISchedulingBus;
        }

        [Test]
        public void SendingSimpleScheduledMessage()
        {
            var startTime = DateTimeOffset.Now.AddSeconds(10);
            _sut.SchedulePublish(time=>time.At(startTime).WithNoRepetition(), new TestMessage(), "Test");
            _mockSerializer.AssertWasCalled(x=>x.MessageToBytes(Arg<ScheduleMe>.Matches(
                m=>m.WakeTime == startTime)));
        }

        [Test]
        public void SchedulingMessageForContinuousRepetitiveSendingEveryHour()
        {
            var startTime = DateTime.Now.AddSeconds(10);
            var interval = TimeSpan.FromHours(1);

            _sut.SchedulePublish(time => time.At(startTime).WithRepetitionEvery(interval).Forever(), new TestMessage(), "Test");
            _mockSerializer.AssertWasCalled(x => x.MessageToBytes(Arg<ScheduleMeRepetitive>.Matches(
                m => (m.WakeTime == startTime &&
                      m.RepetitionInterval == interval &&
                      m.NumberOfRepetitions == -1))));
        }

        [Test]
        public void SchedulingCronMessage()
        {
            var cron = "1 0 * * *";

            _sut.SchedulePublish(x => x.WithCronExpression(cron), new TestMessage(), "TestCron", "Cron");

            _mockSerializer.AssertWasCalled(x => x.MessageToBytes(Arg<ScheduleMeCron>.Matches(
                m => m.CronExpression == cron &&
                     m.Name == "TestCron" &&
                     m.Group == "Cron")));
        }

        [Test]
        public void SchedulingInvalidMessage()
        {
            var paramName = Assert.Throws<ArgumentException>(() => _sut.SchedulePublish(x => { }, new TestMessage(), "TestFail")).ParamName;

            Assert.AreEqual("at", paramName);
        }
    }
}