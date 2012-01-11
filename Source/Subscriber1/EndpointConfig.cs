using EasyNetQ;

namespace Subscriber1
{
    public class EndpointConfig : IConfigureThisEndpoint, IHaveConnectionProperties, IHaveEndpointName
    {
        public EndpointConfig()
        {
            EndpointName = "Subscriber1";
            Host = "localhost";
            VirtualHost = "/";
            Username = "";
            Password = null;
        }

        public string EndpointName { get; private set; }
        public string Host { get; private set; }
        public string VirtualHost { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }
    }
}
