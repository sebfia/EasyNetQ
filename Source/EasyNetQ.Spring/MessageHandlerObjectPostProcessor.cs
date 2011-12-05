using System;
using Common.Logging;
using Spring.Context;
using Spring.Objects.Factory.Config;

namespace EasyNetQ.Spring
{
    public sealed class MessageHandlerObjectPostProcessor : IObjectPostProcessor
    {
        private readonly Lazy<IBus> _bus;
        private readonly Lazy<Endpoint> _endpoint; 
        private readonly ILog _logger;

        public MessageHandlerObjectPostProcessor(IApplicationContext applicationContext)
        {
            _logger = LogManager.GetCurrentClassLogger();
            _bus = new Lazy<IBus>(() => applicationContext.GetObject(typeof(IBus).FullName) as IBus);
            _endpoint = new Lazy<Endpoint>(() => (Endpoint)applicationContext.GetObject(typeof(Endpoint).FullName));
        }

        public object PostProcessBeforeInitialization(object instance, string name)
        {
            return instance;
        }

        public object PostProcessAfterInitialization(object instance, string objectName)
        {
            _logger.Debug(m => m("Post processing '{0}' after initialization", objectName));

            _bus.Value.RegisterHandler(instance, _endpoint.Value.Name);

            return instance;
        }
    }
}