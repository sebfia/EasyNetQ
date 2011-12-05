using System.Configuration;

namespace EasyNetQ.Config
{
    public class EasyNetQConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("xmlns", DefaultValue = "http://schemas.easynetq.com/config", IsRequired = false)]
        public string Namespace//hack to make schema work for config section
        {
            get { return (string) this["xmlns"]; }
            set { this["xmlns"] = value; }
        }

        [ConfigurationProperty("endpoint", IsRequired = false)]
        public EasyNetQMessageEndpointElement Endpoint
        {
            get { return (EasyNetQMessageEndpointElement) this["endpoint"]; }
            set { this["endpoint"] = value; }
        }

        [ConfigurationProperty("connection", IsRequired = false)]
        public EasyNetQConnection Connection
        {
            get { return (EasyNetQConnection) this["connection"]; }
            set { this["connection"] = value; }
        }
    }
}