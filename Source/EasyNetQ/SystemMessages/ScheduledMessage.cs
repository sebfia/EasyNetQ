using System;

namespace EasyNetQ.SystemMessages
{
    [Serializable]
    public abstract class ScheduledMessage
    {
        public string BindingKey { get; set; }
        public byte[] InnerMessage { get; set; }
    }
}