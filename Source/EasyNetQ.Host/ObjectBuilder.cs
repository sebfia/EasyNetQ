using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using EasyNetQ.Host;

namespace EasyNetQ
{
    internal class ObjectBuilder : IBuilder, IConfigureComponents
    {
        private readonly IContainer _container;

        public ObjectBuilder(IContainer container)
        {
            if(container == null)
                throw new ArgumentNullException("container");

            _container = container;
        }

        T IBuilder.Build<T>()
        {
            return (T)_container.Build(typeof(T));
        }

        object IBuilder.Build(Type typeToBuild)
        {
            return _container.Build(typeToBuild);
        }

        IEnumerable<T> IBuilder.BuildAll<T>()
        {
            return _container.BuildAll(typeof(T)).Cast<T>();
        }

        IEnumerable<object> IBuilder.BuildAll(Type typeToBuild)
        {
            return _container.BuildAll(typeToBuild);
        }

        void IBuilder.BuildAndDispatch(Type typeToBuild, Action<object> action)
        {
            action(_container.Build(typeToBuild));
        }

        IConfigureComponents IConfigureComponents.ConfigureProperty<T>(Expression<Func<T, object>> property, object value)
        {
            _container.ConfigureProperty(typeof(T), property.GetPropertyInfoFromExpression().Name, value);
            return this;
        }

        IConfigureComponents IConfigureComponents.RegisterSingleton<T>(object instance)
        {
            return ((IConfigureComponents)this).RegisterSingleton(instance.GetType(), instance);
        }

        IConfigureComponents IConfigureComponents.RegisterSingleton(Type lookupType, object instance)
        {
            _container.Configure(lookupType, ComponentCallModelEnum.Singleton);
            _container.RegisterSingleton(lookupType, instance);
            return this;
        }

        private bool _disposed;
        void IDisposable.Dispose()
        {
            if (_disposed) return;

            if (_container != null) _container.Dispose();

            _disposed = true;
        }
    }
}
