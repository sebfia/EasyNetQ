using System;
using System.Collections.Generic;
using Common.Logging;
using Quartz;
using Quartz.Impl.AdoJobStore;
using Quartz.Simpl;
using Quartz.Spi;

namespace EasyNetQ.QuartzScheduler
{
    public class MyJobStore : JobStoreTX
    {
        private readonly ILog _logger;

        public MyJobStore()
        {
            _logger = LogManager.GetCurrentClassLogger();
            _logger.Debug(m => m("MyJobStore initialized!"));
        }

        public override IList<IOperableTrigger> AcquireNextTriggers(DateTimeOffset noLaterThan, int maxCount, TimeSpan timeWindow)
        {
            _logger.Info(m => m("AcquireNextTriggers"));

            try
            {
                return base.AcquireNextTriggers(noLaterThan, maxCount, timeWindow);
            }
            catch (Exception exception)
            {
                _logger.Error(m=>m(exception.ToString()));
                throw;
            }
        }
    }

    public class SpringEnhancedJobFactory : PropertySettingJobFactory
    {
        private readonly IRawByteBus _bus;

        public SpringEnhancedJobFactory(IRawByteBus bus)
        {
            _bus = bus;
        }

        public override void SetObjectProperties(object obj, JobDataMap data)
        {
            base.SetObjectProperties(obj, data);

            if (obj is SendMessageJob)
            {
                (obj as SendMessageJob).Bus = _bus;
            }
        }
    }
}