using System;

namespace EasyNetQ
{
    public interface ISchedulingBus
    {
        void SchedulePublish<T>(Action<ISchedulePublishConfigurer> at, T message, string name, string group = null);
        void UnschedulePublishedMessage(string name, string group = null);
    }
}