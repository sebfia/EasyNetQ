using System;
using System.Text;
using Common.Logging;
using Quartz;

namespace EasyNetQ.QuartzScheduler
{
    public class SendMessageJob : IJob
    {
        private readonly ILog _logger;

        public SendMessageJob()
        {
            _logger = LogManager.GetCurrentClassLogger();
        }

        public void Execute(IJobExecutionContext context)
        {
            _logger.Debug(m => m("Sending scheduled message from job '{0}' of group '{1}'", 
                                 context.JobDetail.Key.Name, 
                                 context.JobDetail.Key.Group));

            try
            {
                Bus.RawPublish(BindingKey, InnerMessage);

                _logger.Debug(m => m("Scheduled message has been sent."));

                if (context.NextFireTimeUtc != null)
                    _logger.Debug(m => m("This job will execute again on {0} at {1}Z", 
                        context.NextFireTimeUtc.Value.Date.ToShortDateString(), 
                        context.NextFireTimeUtc.Value.TimeOfDay));
                else
                {
                    _logger.Debug(m => m("This job will not execute again."));
                }
            }
            catch (Exception exception)
            {
                _logger.Error(m => m("Error sending scheduled message!"), exception);
            }
        }

        public IRawByteBus Bus { get; set; }
        public string BindingKey { get; set; }
        public byte[] InnerMessage { get; set; }
    }
}