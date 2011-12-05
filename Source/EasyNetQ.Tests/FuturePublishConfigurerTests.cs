using System;
using NUnit.Framework;

namespace EasyNetQ.Tests
{
    [TestFixture]
    public class SchedulePublishConfigurerTests
    {
        private SchedulePublishConfigurer _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new SchedulePublishConfigurer();
        }

        [Test]
        public void InitiallyInvalid()
        {
            Assert.IsFalse(_sut.IsValid);
        }

        [Test]
        public void SettingStartTimeOnly()
        {
            var startTime = DateTimeOffset.Now.AddSeconds(1);
            _sut.At(startTime);

            Assert.IsTrue(_sut.IsValid);
            Assert.IsTrue(_sut.IsSimple);
            Assert.IsFalse(_sut.IsRepeat);
            Assert.IsFalse(_sut.IsCron);
            Assert.AreEqual(startTime, _sut.StartTime);
        }

        [Test]
        public void SettingRepeatIntervalAndCount()
        {
            var startTime = DateTimeOffset.Now.AddSeconds(1);
            var repeatInterval = TimeSpan.FromMinutes(1);
            var repeatCount = 10;

            _sut.At(startTime).WithRepetitionEvery(repeatInterval).For(repeatCount);

            Assert.IsTrue(_sut.IsValid);
            Assert.IsTrue(_sut.IsRepeat);
            Assert.IsFalse(_sut.IsSimple);
            Assert.IsFalse(_sut.IsCron);
            Assert.AreEqual(startTime, _sut.StartTime);
            Assert.AreEqual(repeatInterval, _sut.RepeatInterval);
            Assert.AreEqual(repeatCount, _sut.RepeatCount);
        }

        [Test]
        public void SettingRepeatForever()
        {
            var startTime = DateTimeOffset.Now.AddSeconds(1);
            var repeatInterval = TimeSpan.FromMinutes(1);

            _sut.At(startTime).WithRepetitionEvery(repeatInterval).Forever();

            Assert.IsTrue(_sut.IsValid);
            Assert.IsTrue(_sut.IsRepeat);
            Assert.IsFalse(_sut.IsSimple);
            Assert.IsFalse(_sut.IsCron);
            Assert.AreEqual(startTime, _sut.StartTime);
            Assert.AreEqual(repeatInterval, _sut.RepeatInterval);
            Assert.AreEqual(-1, _sut.RepeatCount);
        }

        [Test]
        public void SettingCronExpression()
        {
            var cron = "1 0 * * *";

            _sut.WithCronExpression(cron);

            Assert.IsTrue(_sut.IsValid);
            Assert.IsTrue(_sut.IsCron);
            Assert.IsFalse(_sut.IsSimple);
            Assert.IsFalse(_sut.IsRepeat);
            Assert.AreEqual(cron, _sut.CronExpression);
        }

        [Test]
        public void Test()
        {
            var startTime = DateTimeOffset.Now.AddSeconds(10);

            _sut.At(startTime).WithNoRepetition();

            Assert.IsTrue(_sut.IsValid);
            Assert.IsTrue(_sut.IsSimple);
            Assert.IsFalse(_sut.IsRepeat);
            Assert.IsFalse(_sut.IsCron);
            Assert.AreEqual(startTime, _sut.StartTime);
        }
    }
}