using Autobus;
using BeatTogether.DedicatedServer.Interface.Events;
using BeatTogether.DedicatedServer.Kernel.Encryption;
using BeatTogether.DedicatedServer.Kernel.Managers.Abstractions;
using BeatTogether.DedicatedServer.Node.Abstractions;
using BeatTogether.DedicatedServer.Node.Configuration;
using BeatTogether.MasterServer.Interface.Events;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Node
{
    public sealed class ApiServerEventHandler : IHostedService
    {
        private readonly IAutobus _autobus;
        private readonly ILogger _logger = Log.ForContext<ApiServerEventHandler>();
        private readonly IInstanceRegistry _instanceRegistry;

        public ApiServerEventHandler(
            IAutobus autobus,
            IInstanceRegistry instanceRegistry)
        {
            _autobus = autobus;
            _instanceRegistry = instanceRegistry;
        }

        #region Public Methods

        public Task StartAsync(CancellationToken cancellationToken)
        {
            /*
            _autobus.Subscribe<PlayerConnectedToMatchmakingServerEvent>(HandlePlayerConnectedToMatchmaking);
            _autobus.Subscribe<CheckNodesEvent>(HandleCheckNode);
            _autobus.Subscribe<DisconnectPlayerFromMatchmakingServerEvent>(HandleDisconnectPlayer);
            _autobus.Subscribe<CloseServerInstanceEvent>(HandleCloseServer);
            */
            return Task.CompletedTask;

        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            /*
            _autobus.Unsubscribe<PlayerConnectedToMatchmakingServerEvent>(HandlePlayerConnectedToMatchmaking);
            _autobus.Unsubscribe<CheckNodesEvent>(HandleCheckNode);
            _autobus.Unsubscribe<DisconnectPlayerFromMatchmakingServerEvent>(HandleDisconnectPlayer);
            _autobus.Unsubscribe<CloseServerInstanceEvent>(HandleCloseServer);
            */
            return Task.CompletedTask;
        }

        #endregion

        #region AutobusEventHandlers
        /*
        private Task HandlePlayerConnectedToMatchmaking(PlayerConnectedToMatchmakingServerEvent @event)
        {

        }
        */

        #endregion
    }
}
