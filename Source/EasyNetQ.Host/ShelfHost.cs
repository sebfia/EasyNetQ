using System;
using System.Threading;
using Topshelf.Messages;
using Topshelf.Model;

namespace EasyNetQ.Host
{
    public class ShelfHost
    {
        private const string __genericServiceName = "GenericService";
        private readonly IServiceChannel _serviceChannel;

        public ShelfHost(IServiceChannel serviceChannel)
        {
            _serviceChannel = serviceChannel;
        }

        private void CreateGenericService()
        {
            var message = new CreateShelfService(__genericServiceName, ShelfType.Internal, typeof(GenericServiceBootstrapper));
            _serviceChannel.Send(message);
        }

        public void Start()
        {
            CreateGenericService();
        }

        public void Stop()
        {
            _serviceChannel.Send(new StopService(__genericServiceName));
            Thread.Sleep(TimeSpan.FromSeconds(3));
            _serviceChannel.Send(new UnloadService(__genericServiceName));
        }

        public void Pause()
        {
            _serviceChannel.Send(new PauseService(__genericServiceName));
        }

        public void Continue()
        {
            _serviceChannel.Send(new ContinueService(__genericServiceName));
        }
    }
}