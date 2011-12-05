// ReSharper disable CheckNamespace
namespace EasyNetQ
{
    public interface IHaveConnectionProperties
    {
        string Host { get; }
        string VirtualHost { get; }
        string Username { get; }
        string Password { get; }
    }
}
// ReSharper restore CheckNamespace