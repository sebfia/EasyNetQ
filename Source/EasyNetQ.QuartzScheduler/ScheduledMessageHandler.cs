using System;
using System.Collections;
using System.Collections.Generic;
using Common.Logging;
using EasyNetQ.SystemMessages;
using Quartz;

namespace EasyNetQ.QuartzScheduler
{
    public class ScheduledMessageHandler : 
        IHandleMessage<ScheduleMe>, 
        IHandleMessage<ScheduleMeRepetitive>, 
        IHandleMessage<ScheduleMeCron>,
        IHandleMessage<UnscheduleMe>
    {
        private readonly IScheduler _scheduler;
        private readonly ILog _logger;

        public ScheduledMessageHandler(IScheduler scheduler)
        {
            if(scheduler == null)
                throw new ArgumentNullException("scheduler");

            _scheduler = scheduler;
            _logger = LogManager.GetCurrentClassLogger();

            if (!_scheduler.IsStarted)
            {
                _logger.Debug(m => m("Starting scheduler."));

                try
                {
                    _scheduler.Start();
                }
                catch (Exception exception)
                {
                    _logger.Error(m => m("An exception was thrown when trying to start the scheduler: {0}", exception.Message));
                    _logger.Error(m => m(exception.ToString()));
                    throw;
                }
            }
        }

        public void Handle(ScheduleMe message)
        {
            _logger.Debug(m => m("Scheduling message to be sent at time {0}Z for one time only.", message.WakeTime));

            try
            {
                var identity = new JobKey(Guid.NewGuid().ToString());

                var jobDetail = CreateJobDetailFromMessage(message, identity);

                _logger.Debug((m => m("Creating trigger.")));

                var trigger = TriggerBuilder
                    .Create()
                    .WithIdentity(identity.Name, identity.Group)
                    .StartAt(message.WakeTime)
                    .WithSimpleSchedule()
                    .Build();

                ScheduleJob(jobDetail, trigger);
            }
            catch (Exception exception)
            {
                _logger.Error(m => m("Error scheduling message due to an exception: {0}", exception.Message));
                _logger.Debug(m => m(exception.ToString()));
            }
        }

        public void Handle(ScheduleMeRepetitive message)
        {
            _logger.Debug(m => m("Scheduling repetitive message to be sent at local time {0} for the first time", message.WakeTime.LocalDateTime));

            try
            {
                var identity = CreateJobKeyFromMessage(message);

                var jobDetail = CreateJobDetailFromMessage(message, identity);

                var trigger = TriggerBuilder
                    .Create()
                    .WithIdentity(identity.Name, identity.Group)
                    .StartAt(message.WakeTime.UtcDateTime)
                    .WithSimpleSchedule(new RepeatableMessageConfigurer(message).ConfigureMessage)
                    .Build();

                ScheduleJob(jobDetail, trigger);
            }
            catch (Exception exception)
            {
                _logger.Error(m => m("Error scheduling message due to an exception: {0}", exception.Message));
                _logger.Debug(m => m(exception.ToString()));
            }
        }

        public void Handle(ScheduleMeCron message)
        {
            _logger.Debug(m => m("Scheduling message to be sent by a cron schedule. Cron expression: '{0}'", message.CronExpression));

            try
            {
                var identity = CreateJobKeyFromMessage(message);

                var jobDetail = CreateJobDetailFromMessage(message, identity);

                var trigger = TriggerBuilder
                    .Create()
                    .WithIdentity(identity.Name, identity.Group)
                    .WithCronSchedule(message.CronExpression)
                    .Build();

                ScheduleJob(jobDetail, trigger);
            }
            catch (Exception exception)
            {
                _logger.Error(m => m("Error scheduling message due to an exception: {0}", exception.Message));
                _logger.Debug(m => m(exception.ToString()));
            }
        }

        public void Handle(UnscheduleMe message)
        {
            try
            {
                var jobKey = CreateJobKeyFromMessage(message);

                _logger.Debug(m => m("Unscheduling message stored under name: {0} and group: {1}",
                    jobKey.Name,
                    jobKey.Group));

                if (!_scheduler.CheckExists(jobKey))
                {
                    _logger.Error(m => m("No scheduled message registered with name: {0} and group: {1}",
                                         jobKey.Name,
                                         jobKey.Group));
                    return;
                }

                _scheduler.DeleteJob(jobKey);
            }
            catch (Exception exception)
            {
                _logger.Error(m => m("An exception was thrown when trying to unschedule a scheduled message: {0}", exception.Message));
                _logger.Debug(m => m(exception.ToString()));
            }
        }

        private JobKey CreateJobKeyFromMessage(IAssociable message)
        {
            return String.IsNullOrEmpty(message.Group)
                       ? new JobKey(message.Name)
                       : new JobKey(message.Name, message.Group);
        }

        private IDictionary CreateDataMapFromMessage(ScheduledMessage message)
        {
            if(message == null)
                throw new ArgumentNullException("message");

            return new Dictionary<string, object>
                                 {
                                     {"BindingKey", message.BindingKey},
                                     {"InnerMessage", message.InnerMessage}
                                 };
        }

        private IJobDetail CreateJobDetailFromMessage(ScheduledMessage message, JobKey identity)
        {
            _logger.Debug(m => m("Creating job detail."));

            return JobBuilder
                .Create<SendMessageJob>()
                .UsingJobData(new JobDataMap(CreateDataMapFromMessage(message)))
                .WithIdentity(identity)
                .Build();
        }

        private void ScheduleJob(IJobDetail jobDetail, ITrigger trigger)
        {
            _logger.Debug(m => m("Scheduling job."));

            try
            {
                _scheduler.ScheduleJob(jobDetail, trigger);
            }
            catch (Exception exception)
            {
                _logger.Error(m => m("An exception was thrown when trying to schedule new job: {0}", exception.Message));
                _logger.Info(m => m(exception.ToString()));
            }
        }
    }
}