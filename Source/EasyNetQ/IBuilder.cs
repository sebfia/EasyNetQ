using System;
using System.Collections.Generic;

namespace EasyNetQ
{
    public interface IBuilder : IDisposable
    {
        T Build<T>();
        object Build(Type typeToBuild);
        IEnumerable<T> BuildAll<T>();
        IEnumerable<object> BuildAll(Type typeToBuild);
        void BuildAndDispatch(Type typeToBuild, Action<object> action);
    }
}