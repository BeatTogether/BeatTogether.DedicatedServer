using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Requests;
using Microsoft.Extensions.Hosting;
using Obvs;
using Serilog;

namespace BeatTogether.DedicatedServer.Kernel.Implementations.MessageHandlers
{
    public class DedicatedServerMessageHandler : IHostedService
    {
        private readonly IServiceBus _serviceBus;
        private readonly IDedicatedServerService _dedicatedServerService;
        private readonly List<IDisposable> _subscriptions;

        public DedicatedServerMessageHandler(
            IServiceBus serviceBus,
            IDedicatedServerService dedicatedServerService)
        {
            _serviceBus = serviceBus;
            _dedicatedServerService = dedicatedServerService;
            _subscriptions = new();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _subscriptions.Add(_serviceBus.Requests
                .OfType<GetAvailableRelayServerRequest>()
                .Subscribe(async request =>
                {
                    Log.Information($"Got message '{request.SourceEndPoint}', '{request.TargetEndPoint}'");
                    var response = await _dedicatedServerService.GetAvailableRelayServer(request);
                    await _serviceBus.ReplyAsync(request, response);
                })
            );
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            foreach (var subscription in _subscriptions)
                subscription.Dispose();
            return Task.CompletedTask;
        }
    }
}
