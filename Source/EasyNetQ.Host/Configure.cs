using System;

namespace EasyNetQ
{
    public class Configure
    {
        private static readonly Configure _instance;
        private ObjectBuilder _builder;
        private IConfigureComponents _configureComponents;
        private readonly IBusConfigurer _busConfigurer;
        private readonly IBusBuilder _busBuilder;
        
        protected Configure()
        {
            Container = new StandardContainer();
            _busConfigurer = new BusBuilder();
            _busBuilder = _busConfigurer as IBusBuilder;
        }

        static Configure()
        {
            _instance = new Configure();
        }

        internal static Configure Instance
        {
            get { return _instance; }
        }

        internal IContainer Container
        {
            set
            {
                if(value == null)
                    throw new ArgumentNullException("value");

                _builder = new ObjectBuilder(value);
                _configureComponents = _builder;
            }
        }

        internal IBuilder Builder
        {
            get { return _builder; }
        }

        internal IBusConfigurer Bus
        {
            get { return _busConfigurer; }
        }

        internal IConfigureComponents ConfigureComponents
        {
            get { return _configureComponents; }
        }

        internal IBusBuilder BusBuilder
        {
            get { return _busBuilder; }
        }

        public static Configure With()
        {
            return _instance;
        }
    }
}
