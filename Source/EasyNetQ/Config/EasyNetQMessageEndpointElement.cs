using System.Configuration;

namespace EasyNetQ.Config
{
    public class EasyNetQMessageEndpointElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = false, DefaultValue = null)]
        public string Name
        {
            get { return (string) this["name"]; }
            set { this["name"] = value; }
        }
    }
}