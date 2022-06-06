using Autobus;
using BeatTogether.DedicatedServer.Interface.Events;
using BeatTogether.DedicatedServer.Kernel.Encryption;
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
        //TODO add in event handling here
        private readonly IAutobus _autobus;
        private readonly PacketEncryptionLayer _packetEncryptionLayer;
        private readonly ILogger _logger = Log.ForContext<MasterServerEventHandler>();
        private readonly NodeConfiguration _configuration;

        public MasterServerEventHandler(
            IAutobus autobus,
            PacketEncryptionLayer packetEncryptionLayer,
            NodeConfiguration nodeConfiguration)
        {
            _autobus = autobus;
            _packetEncryptionLayer = packetEncryptionLayer;
            _configuration = nodeConfiguration;
        }

        #region Public Methods

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _autobus.Subscribe<PlayerConnectedToMatchmakingServerEvent>(HandlePlayerConnectedToMatchmaking);
            _autobus.Subscribe<CheckNodesEvent>(HandleCheckNode);
            _autobus.Publish(new NodeStartedEvent(_configuration.HostName));

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _autobus.Unsubscribe<PlayerConnectedToMatchmakingServerEvent>(HandlePlayerConnectedToMatchmaking);
            _autobus.Unsubscribe<CheckNodesEvent>(HandleCheckNode);
            return Task.CompletedTask;
        }

        #endregion

        #region Private Methods

        private Task HandlePlayerConnectedToMatchmaking(PlayerConnectedToMatchmakingServerEvent @event)
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
            return Task.CompletedTask;
        }

        private Task HandleCheckNode(CheckNodesEvent checkNodesEvent)
        {
            ReturnEvent();
            return Task.CompletedTask;
        }


        private void ReturnEvent()
        {
            _autobus.Publish(new NodeOnlineEvent(_configuration.HostName));
        }
        #endregion
    }
}
