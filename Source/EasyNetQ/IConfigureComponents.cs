using System;
using System.Linq.Expressions;

namespace EasyNetQ
{
    public interface IConfigureComponents
    {
        IConfigureComponents ConfigureProperty<T>(Expression<Func<T, object>> property, object value);
        IConfigureComponents RegisterSingleton<T>(object instance);
        IConfigureComponents RegisterSingleton(Type lookupType, object instance);
    }
}