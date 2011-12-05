// ReSharper disable CheckNamespace
namespace EasyNetQ
{
    public interface IWantToRunAtStartup
    {
        void Start();
        void Stop();
    }
}
// ReSharper restore CheckNamespace