using BeatTogether.DedicatedServer.Kernel.Abstractions;
using Serilog;
using System;
using System.Linq;
using System.Threading;

namespace BeatTogether.DedicatedServer.Kernel
{
    public sealed class ServiceAccessor<IService, TService> : IServiceAccessor<IService>
    {
        private static AsyncLocal<IService> _service = new();
        public IService Service => _service.Value!;

        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger = Log.ForContext<ServiceAccessor<IService, TService>>();

        public ServiceAccessor(
            IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IService Create() 
        {
            var service = (TService)_serviceProvider.GetService(typeof(TService));
            _service.Value = service!;
            return service!;
        }
    }
}
