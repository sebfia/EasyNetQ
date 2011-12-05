using EasyNetQ.Host;
using EasyNetQ.Spring;

namespace EasyNetQ.QuartzScheduler
{
    public class EnpointConfig : IConfigureThisEndpoint, IWantCustomInitialization
    {
        public void Initialize()
        {
            Configure.With()
                .Logger<CommonLoggingEasyNetQLogger>()
                .Container<SpringContainer>();
        }
    }
}
