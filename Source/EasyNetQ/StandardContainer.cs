using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyNetQ
{
    public class StandardContainer : IContainer
    {
        private readonly Dictionary<Type, List<object>> _registeredTypes; 

        public StandardContainer()
        {
            _registeredTypes = new Dictionary<Type, List<object>>();
        }

        public object Build(Type typeToBuild)
        {
            object result = null;

            if (typeToBuild.IsAbstract || typeToBuild.IsInterface)
            {
                var types = ScanAssemblies.For(typeToBuild);

                Type firstFound;

                if (types != null && ((firstFound = types.FirstOrDefault()) != null))
                {
                    result =  Activator.CreateInstance(firstFound);
                }

                throw new InvalidOperationException("Can not build an instance of type: '"+typeToBuild.Name+"' because no member is implementing it!");
            }
            else
            {
                result = Activator.CreateInstance(typeToBuild);                
            }

            return result;
        }

        public IEnumerable<object> BuildAll(Type typeToBuild)
        {
            if (typeToBuild.IsAbstract || typeToBuild.IsInterface)
            {
                var types = ScanAssemblies.For(typeToBuild);

                foreach (var instance in types.Select(Activator.CreateInstance))
                {
                    if (instance is IWantTheBus)
                    {
                        if ( _registeredTypes.ContainsKey(typeof(IBus)))
                            (instance as IWantTheBus).Bus = _registeredTypes[typeof (IBus)][0] as IBus;
                    }

                    yield return instance;
                }
            }
        }

        public void Configure(Type component, ComponentCallModelEnum callModel)
        {
            
        }

        public void ConfigureProperty(Type component, string property, object value)
        {
            
        }

        public void RegisterSingleton(Type lookupType, object instance)
        {
            if (!_registeredTypes.ContainsKey(lookupType))
            {
                _registeredTypes.Add(lookupType, new List<object>());
            }

            if (!_registeredTypes[lookupType].Contains(instance))
            {
                _registeredTypes[lookupType].Add(instance);
            }
        }

        void IDisposable.Dispose()
        {
            
        }
    }
}