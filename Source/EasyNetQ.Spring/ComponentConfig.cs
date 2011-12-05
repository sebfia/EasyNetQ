using System.Collections.Generic;
using Spring.Objects.Factory.Support;

namespace EasyNetQ.Spring
{
    internal class ComponentConfig : IComponentConfig
    {
        private readonly Dictionary<string, object> _properties = new Dictionary<string, object>();

        public void Configure(ObjectDefinitionBuilder builder)
        {
            foreach (string key in _properties.Keys)
            {
                builder.AddPropertyValue(key, _properties[key]);
            }
        }

        public IComponentConfig ConfigureProperty(string name, object value)
        {
            _properties[name] = value;
            return this;
        }
    }
}