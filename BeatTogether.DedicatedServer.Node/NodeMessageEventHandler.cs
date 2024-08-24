using System.Threading;
using System.Threading.Tasks;
using Autobus;
using BeatTogether.Core.Abstractions;
using BeatTogether.Core.Extensions;
using BeatTogether.Core.ServerMessaging.Models;
using BeatTogether.DedicatedServer.Interface.Events;
using BeatTogether.DedicatedServer.Node.Configuration;
using BeatTogether.DedicatedServer.Node.Models;
using BeatTogether.MasterServer.Interface.Events;
using BinaryRecords;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace BeatTogether.DedicatedServer.Node
{
    public sealed class NodeMessageEventHandler : IHostedService
    {
        private readonly IAutobus _autobus;
        private readonly ILogger _logger = Log.ForContext<NodeMessageEventHandler>();
        private readonly NodeConfiguration _configuration;
        private readonly ILayer2 _Layer2;

        public NodeMessageEventHandler(
            IAutobus autobus,
            NodeConfiguration nodeConfiguration,
            ILayer2 layer2)
        {
            _autobus = autobus;
            _configuration = nodeConfiguration;
            _Layer2 = layer2;

            BinarySerializer.AddGeneratorProvider(
                (Player value, ref BinaryBufferWriter buffer) => BinaryBufferWriterExtensions.WritePlayer(ref buffer, value),
                (ref BinaryBufferReader bufferReader) => BinaryBufferReaderExtensions.ReadPlayer(ref bufferReader)
            );
            BinarySerializer.AddGeneratorProvider(
                (Server value, ref BinaryBufferWriter buffer) => BinaryBufferWriterExtensions.WriteServer(ref buffer, value),
                (ref BinaryBufferReader bufferReader) => BinaryBufferReaderExtensions.ReadServer(ref bufferReader)
            );
        }

        #region Start/Stop

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _autobus.Subscribe<PlayerSessionDataSendToDediEvent>(HandlePlayerConnectedToMatchmaking);
            _autobus.Subscribe<CheckNodesEvent>(HandleCheckNode);
            _autobus.Subscribe<DisconnectPlayerFromMatchmakingServerEvent>(HandleDisconnectPlayer);
            _autobus.Subscribe<CloseServerInstanceEvent>(HandleCloseServer);
            _autobus.Publish(new NodeStartedEvent(_configuration.HostEndpoint, _configuration.NodeVersion.ToString()));
            _logger.Information("Dedicated node version: " + _configuration.NodeVersion.ToString() + ". Host Endpoint: " + _configuration.HostEndpoint);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _autobus.Unsubscribe<PlayerSessionDataSendToDediEvent>(HandlePlayerConnectedToMatchmaking);
            _autobus.Unsubscribe<CheckNodesEvent>(HandleCheckNode);
            _autobus.Unsubscribe<DisconnectPlayerFromMatchmakingServerEvent>(HandleDisconnectPlayer);
            _autobus.Unsubscribe<CloseServerInstanceEvent>(HandleCloseServer);
            return Task.CompletedTask;
        }

        #endregion

        #region Handlers

        private async Task HandlePlayerConnectedToMatchmaking(PlayerSessionDataSendToDediEvent SessionDataEvent)
        {
            if (SessionDataEvent.NodeEndpoint != _configuration.HostEndpoint)
                return;

            Core.Abstractions.IPlayer player = new PlayerFromMessage(SessionDataEvent.Player);
            if (!await _Layer2.SetPlayerSessionData(SessionDataEvent.serverInstanceSecret, player))
                return;

            _autobus.Publish(new NodeReceivedPlayerSessionDataEvent(_configuration.HostEndpoint, SessionDataEvent.Player.PlayerSessionId));
            return;
        }

        private Task HandleCheckNode(CheckNodesEvent checkNodesEvent)
        {
            _autobus.Publish(new NodeOnlineEvent(_configuration.HostEndpoint, _configuration.NodeVersion.ToString()));
            return Task.CompletedTask;
        }

        private async Task HandleDisconnectPlayer(DisconnectPlayerFromMatchmakingServerEvent disconnectEvent)
        {
            await _Layer2.DisconnectPlayer(disconnectEvent.Secret, disconnectEvent.HashedUserId);
        }
        
        private async Task HandleCloseServer(CloseServerInstanceEvent closeEvent)
        {
            await _Layer2.CloseInstance(closeEvent.Secret);
        }
        
        #endregion
    }
}
