using System;
using System.Collections;
using System.Collections.Generic;
using Spring.Context.Support;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;

namespace EasyNetQ.Spring
{
    public class SpringContainer : IContainer
    {
        private AbstractApplicationContext _context;
        private readonly Dictionary<Type, ComponentConfig> _componentDefinitions;
        private readonly Dictionary<Type, ComponentCallModelEnum> _callModels; 
        private readonly DefaultObjectDefinitionFactory _factory;
        private bool _isInitialized;

        public SpringContainer()
            : this(new GenericApplicationContext())
        {
        }

        public SpringContainer(GenericApplicationContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            _context = context;
            _componentDefinitions = new Dictionary<Type, ComponentConfig>();
            _callModels = new Dictionary<Type, ComponentCallModelEnum>();
            _factory = new DefaultObjectDefinitionFactory();
        }

        public void Initialize()
        {
            if (!_isInitialized)
            {
                foreach (KeyValuePair<Type, ComponentConfig> componentDefinition in _componentDefinitions)
                {
                    RegisterObjectDefinitionInContext(componentDefinition.Key, componentDefinition.Value,
                                                      _callModels[componentDefinition.Key]);
                }

                var newContext = new EasyNetQApplicationContext(_context);
                newContext.Refresh();

                _context = newContext;

                _isInitialized = true;
            }
        }

        private void RegisterObjectDefinitionInContext(Type objectType, ComponentConfig componentConfig, ComponentCallModelEnum callModelEnum)
        {
            var builder = ObjectDefinitionBuilder.RootObjectDefinition(_factory, objectType)
                        .SetAutowireMode(AutoWiringMode.AutoDetect)
                        .SetSingleton(callModelEnum == ComponentCallModelEnum.Singleton);
            componentConfig.Configure(builder);
            IObjectDefinition objectDefinition = builder.ObjectDefinition;
            _context.RegisterObjectDefinition(objectType.FullName, objectDefinition);
        }

        object IContainer.Build(Type typeToBuild)
        {
            Initialize();
            IDictionary dict = _context.GetObjectsOfType(typeToBuild, true, false);
            if (dict.Count == 0)
            {
                return null;
            }
            IDictionaryEnumerator de = dict.GetEnumerator();
            if (!de.MoveNext())
            {
                return null;
            }
            return de.Value;
        }

        IEnumerable<object> IContainer.BuildAll(Type typeToBuild)
        {
            Initialize();
            IDictionaryEnumerator enumerator = _context.GetObjectsOfType(typeToBuild, true, false).GetEnumerator();
            while (true)
            {
                if (!enumerator.MoveNext())
                {
                    yield break;
                }
                yield return enumerator.Entry.Value;
            }
        }

        void IContainer.Configure(Type component, ComponentCallModelEnum callModel)
        {
            _callModels[component] = callModel;
            lock (_componentDefinitions)
            {
                if (!_componentDefinitions.ContainsKey(component))
                {
                    _componentDefinitions[component] = new ComponentConfig();
                }
            }

        }

        void IContainer.ConfigureProperty(Type component, string property, object value)
        {
            lock (_componentDefinitions)
            {
                ComponentConfig result;
                _componentDefinitions.TryGetValue(component, out result);
                if (result == null)
                {
                    throw new InvalidOperationException("Cannot configure property before the component has been configured. Please call 'Configure' first.");
                }
                result.ConfigureProperty(property, value);
            }

        }

        void IContainer.RegisterSingleton(Type lookupType, object instance)
        {
            _context.ObjectFactory.RegisterSingleton(lookupType.FullName, instance);
        }

        public void AttachMessageHandlersToBus(IBus bus, string endpointName)
        {
            //do nothing here since we are using the MessageHandlerObjectPostProcessor for wiring.
        }

        private bool _disposed;
        void IDisposable.Dispose()
        {
            if (!_disposed && _context != null)
            {
                if (_context.IsRunning)
                    _context.Stop();
                _context.Dispose();
                _disposed = true;
            }
        }
    }
}