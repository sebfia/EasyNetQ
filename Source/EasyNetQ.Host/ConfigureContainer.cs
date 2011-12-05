using System;

// ReSharper disable CheckNamespace
namespace EasyNetQ
{
    public static class ConfigureContainer
    {
        public static Configure Container<TContainer>(this Configure value) where TContainer : class , IContainer, new()
        {
            return value.Container(new TContainer());
        }

        public static Configure Container<TContainer>(this Configure value, TContainer container) where TContainer : class, IContainer
        {
            if(container == null)
                throw new ArgumentNullException("container", "The container must not be null!");

            value.Container = container;

            return value;
        }
    }
}
// ReSharper restore CheckNamespace
