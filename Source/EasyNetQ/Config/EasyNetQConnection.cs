using System;
using System.Configuration;

namespace EasyNetQ.Config
{
    public class EasyNetQConnection : ConfigurationElement
    {
        private static readonly Lazy<EasyNetQConnection> _defaultConnection =
            new Lazy<EasyNetQConnection>(() => new EasyNetQConnection());

        public static EasyNetQConnection DefaultConnection
        {
            get { return _defaultConnection.Value; }
        }

        [ConfigurationProperty("host", IsRequired = true, DefaultValue = "localhost")]
        public string Host
        {
            get { return (string)this["host"]; }
            set { this["host"] = value; }
        }

        [ConfigurationProperty("virtualHost", IsRequired = false, DefaultValue = "/")]
        public string VirtualHost
        {
            get { return (string)this["virtualHost"]; }
            set { this["virtualHost"] = value; }
        }

        [ConfigurationProperty("username", IsRequired = false, DefaultValue = "guest")]
        public string Username
        {
            get { return (string)this["username"]; }
            set { this["username"] = value; }
        }

        [ConfigurationProperty("password", IsRequired = false, DefaultValue = "guest")]
        public string Password
        {
            get { return (string)this["password"]; }
            set { this["password"] = value; }
        }

        [ConfigurationProperty("timeout", IsRequired = false, DefaultValue = 10)]
        public int Timeout
        {
            get { return (int)this["timeout"]; }
            set { this["timeout"] = value; }
        }
    }
}