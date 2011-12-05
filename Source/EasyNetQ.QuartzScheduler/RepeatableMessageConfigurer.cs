using System;
using EasyNetQ.SystemMessages;
using Quartz;

namespace EasyNetQ.QuartzScheduler
{
    public class RepeatableMessageConfigurer
    {
        private readonly ScheduleMeRepetitive _scheduleMeRepetitive;

        public RepeatableMessageConfigurer(ScheduleMeRepetitive scheduleMeRepetitive)
        {
            if (scheduleMeRepetitive == null)
                throw new ArgumentNullException("scheduleMeRepetitive");

            _scheduleMeRepetitive = scheduleMeRepetitive;
        }

        public void ConfigureMessage(SimpleScheduleBuilder scheduleBuilder)
        {
            scheduleBuilder.WithIntervalInSeconds((int) _scheduleMeRepetitive.RepetitionInterval.TotalSeconds);

            if(_scheduleMeRepetitive.NumberOfRepetitions >= 0)
                scheduleBuilder.WithRepeatCount(_scheduleMeRepetitive.NumberOfRepetitions);
            if(_scheduleMeRepetitive.NumberOfRepetitions < 0)
                scheduleBuilder.RepeatForever();
        }
    }
}