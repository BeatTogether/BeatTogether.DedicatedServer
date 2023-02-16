using Autobus;
using BeatTogether.DedicatedServer.Interface.Events;
using BeatTogether.DedicatedServer.Kernel.Encryption;
using BeatTogether.DedicatedServer.Node.Abstractions;
using BeatTogether.DedicatedServer.Node.Configuration;
using BeatTogether.MasterServer.Interface.Events;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Node
{
    public sealed class MasterServerEventHandler : IHostedService
    {
        private readonly IAutobus _autobus;
        private readonly PacketEncryptionLayer _packetEncryptionLayer;
        private readonly ILogger _logger = Log.ForContext<MasterServerEventHandler>();
        private readonly NodeConfiguration _configuration;
        private readonly IInstanceRegistry _instanceRegistry;

        public MasterServerEventHandler(
            IAutobus autobus,
            PacketEncryptionLayer packetEncryptionLayer,
            NodeConfiguration nodeConfiguration,
            IInstanceRegistry instanceRegistry)
        {
            _autobus = autobus;
            _packetEncryptionLayer = packetEncryptionLayer;
            _configuration = nodeConfiguration;
            _instanceRegistry = instanceRegistry;
        }

        #region Public Methods

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _autobus.Subscribe<PlayerConnectedToMatchmakingServerEvent>(HandlePlayerConnectedToMatchmaking);
            _autobus.Subscribe<CheckNodesEvent>(HandleCheckNode);
            _autobus.Subscribe<DisconnectPlayerFromMatchmakingServerEvent>(HandleDisconnectPlayer);
            _autobus.Subscribe<CloseServerInstanceEvent>(HandleCloseServer);
            _autobus.Publish(new NodeStartedEvent(_configuration.HostName, _configuration.NodeVersion));
            _logger.Information("Dedicated node version: " + _configuration.NodeVersion + " starting: " + _configuration.HostName);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _autobus.Unsubscribe<PlayerConnectedToMatchmakingServerEvent>(HandlePlayerConnectedToMatchmaking);
            _autobus.Unsubscribe<CheckNodesEvent>(HandleCheckNode);
            _autobus.Unsubscribe<DisconnectPlayerFromMatchmakingServerEvent>(HandleDisconnectPlayer);
            _autobus.Unsubscribe<CloseServerInstanceEvent>(HandleCloseServer);
            return Task.CompletedTask;
        }

        #endregion

        #region Private Methods

        private Task HandlePlayerConnectedToMatchmaking(PlayerConnectedToMatchmakingServerEvent @event)
        {
            if(@event.NodeEndpoint == _configuration.HostName)
            {
                var remoteEndPoint = IPEndPoint.Parse(@event.RemoteEndPoint);
                var random = @event.Random;
                var publicKey = @event.PublicKey;
                _logger.Verbose(
                    "Adding encrypted end point " +
                    $"(RemoteEndPoint='{remoteEndPoint}', " +
                    $"Random='{BitConverter.ToString(random)}', " +
                    $"PublicKey='{BitConverter.ToString(publicKey)}')."
                );
                _packetEncryptionLayer.AddEncryptedEndPoint(remoteEndPoint, random, publicKey);
                _autobus.Publish(new NodeReceivedPlayerEncryptionEvent(_configuration.HostName, @event.RemoteEndPoint));
            }
            return Task.CompletedTask;
        }

        private Task HandleCheckNode(CheckNodesEvent checkNodesEvent)
        {
            _autobus.Publish(new NodeOnlineEvent(_configuration.HostName, _configuration.NodeVersion));
            return Task.CompletedTask;
        }

        private Task HandleDisconnectPlayer(DisconnectPlayerFromMatchmakingServerEvent disconnectEvent)
        {
            if(_instanceRegistry.TryGetInstance(disconnectEvent.Secret, out var instance))
                instance.DisconnectPlayer(disconnectEvent.UserId);
            return Task.CompletedTask;
        }
        private Task HandleCloseServer(CloseServerInstanceEvent closeEvent)
        {
            if (_instanceRegistry.TryGetInstance(closeEvent.Secret, out var instance))
            {
                instance.Stop();
            }
            return Task.CompletedTask;
        }
        #endregion
    }
}
