using System;
using System.Reflection;

namespace EasyNetQ
{
    public static class ExtendsBus
    {
        private static readonly Type _actionType = typeof (Action<>);
        private static readonly MethodInfo _subscribeMethod = typeof(IBus).GetMethod("Subscribe", BindingFlags.Public|BindingFlags.Instance);
        private static readonly Type _handleMessageType = typeof(IHandleMessage<>);

        public static void RegisterHandler(this IBus bus, object handler, string subscriptionId)
        {
            if(bus == null)
                throw new ArgumentNullException("bus");
            if(handler == null)
                throw new ArgumentNullException("handler");

            var handlerType = handler.GetType();


            foreach (var @interface in handlerType.GetInterfaces())
            {
                if (@interface.IsGenericType && @interface.GetGenericTypeDefinition() == _handleMessageType)
                {
                    var method = @interface.GetMethods()[0];
                    var parameter = method.GetParameters()[0];
                    var endType = _actionType.MakeGenericType(parameter.ParameterType);

                    var dlg = Delegate.CreateDelegate(endType, handler, method);

                    var genericMethod = _subscribeMethod.MakeGenericMethod(parameter.ParameterType);
                    genericMethod.Invoke(bus, new object[] {subscriptionId, dlg});
                }
            }
        }
    }
}