using System;

namespace EasyNetQ.SystemMessages
{
    [Serializable]
    public class ScheduleMeCron : ScheduledMessage, IAssociable
    {
        public string Name { get; set; }
        public string Group { get; set; }
        public string CronExpression { get; set; }
    }
}