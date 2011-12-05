using System;

namespace EasyNetQ
{
    public static class ConfigureEndpoint
    {
        public static Configure EndpointName(this Configure value, string name)
        {
            if(value == null)
                throw new ArgumentNullException("value");

            value.ConfigureComponents.RegisterSingleton<Endpoint>(new Endpoint {Name = name});

            return value;
        }
    }
}
