using System;
using Contracts;
using EasyNetQ;

namespace Subscriber1
{
    public class MagicMessageHandler : IHandleMessage<Message>
    {
        public void Handle(Message message)
        {
            Console.WriteLine("Received message with text: '{0}'", message.Text);
        }
    }
}