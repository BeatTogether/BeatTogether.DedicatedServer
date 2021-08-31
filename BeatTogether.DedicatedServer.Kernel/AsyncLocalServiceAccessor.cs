using BeatTogether.DedicatedServer.Kernel.Abstractions;
using System;
using System.Threading;

namespace BeatTogether.DedicatedServer.Kernel
{
    public sealed class AsyncLocalServiceAccessor<TService> : IAsyncLocalServiceAccessor<TService>
    {
        private static AsyncLocal<TService> _service = new();
        public TService Service => _service.Value!;

        public TService Set(TService service)
        {
            _service.Value = service;
            return service;
        }
    }
}
