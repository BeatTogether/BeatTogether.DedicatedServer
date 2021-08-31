using BeatTogether.DedicatedServer.Kernel.Abstractions;
using Serilog;
using System;
using System.Linq;
using System.Threading;

namespace BeatTogether.DedicatedServer.Kernel
{
    public sealed class ServiceAccessor<TService> : IServiceAccessor<TService>
    {
        private static AsyncLocal<TService> _service = new();
        public TService Service => _service.Value!;

        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger = Log.ForContext<ServiceAccessor<TService>>();

        public ServiceAccessor(
            IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public TService Create<ServiceType>() where ServiceType : TService
        {
            var constructor = typeof(ServiceType).GetConstructors()[0];
            var paramTypes = constructor.GetParameters().Select(parameter => parameter.ParameterType);
            var parameters = paramTypes.Select(type => _serviceProvider.GetService(type));
            var service = (TService)constructor.Invoke(parameters.ToArray());
            _service.Value = service;
            return service;
        }
    }
}
