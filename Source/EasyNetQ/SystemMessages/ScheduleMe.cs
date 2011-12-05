using System;

namespace EasyNetQ.SystemMessages
{
    [Serializable]
    public class ScheduleMe : ScheduledMessage
    {
        public DateTime WakeTime { get; set; }
    }
}