using EasyNetQ.Config;
using NUnit.Framework;

namespace EasyNetQ.Tests
{
    [TestFixture]
    public class ConfigurationFixture
    {
        [Test]
        public void CheckDefaultConnectionProperties()
        {
            var sut = EasyNetQConnection.DefaultConnection;
            Assert.AreEqual("localhost", sut.Host);
            Assert.AreEqual("/", sut.VirtualHost);
            Assert.AreEqual("guest", sut.Username);
            Assert.AreEqual("guest", sut.Password);
        }
    }
}