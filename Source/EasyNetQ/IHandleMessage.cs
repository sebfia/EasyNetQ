// ReSharper disable CheckNamespace
namespace EasyNetQ
{
    public interface IHandleMessage<in TMessage>
    {
        void Handle(TMessage message);
    }
}
// ReSharper restore CheckNamespace