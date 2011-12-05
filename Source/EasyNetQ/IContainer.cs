using System;
using System.Collections.Generic;

namespace EasyNetQ
{
    public interface IContainer : IDisposable
    {
        object Build(Type typeToBuild);
        IEnumerable<object> BuildAll(Type typeToBuild);
        void Configure(Type component, ComponentCallModelEnum callModel);
        void ConfigureProperty(Type component, string property, object value);
        void RegisterSingleton(Type lookupType, object instance);
    }
}
