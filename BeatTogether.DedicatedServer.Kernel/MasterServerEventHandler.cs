using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Autobus;
using BeatTogether.MasterServer.Interface.Events;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace BeatTogether.DedicatedServer.Kernel
{
    public sealed class MasterServerEventHandler : IHostedService
    {
        private readonly IAutobus _autobus;
        private readonly PacketEncryptionLayer _packetEncryptionLayer;
        private readonly ILogger _logger = Log.ForContext<MasterServerEventHandler>();

        public MasterServerEventHandler(
            IAutobus autobus,
            PacketEncryptionLayer packetEncryptionLayer)
        {
            _autobus = autobus;
            _packetEncryptionLayer = packetEncryptionLayer;
        }

        #region Public Methods

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _autobus.Subscribe<PlayerConnectedToMatchmakingServerEvent, PlayerConnectedToMatchmakingServerAck>(Handle);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _autobus.Unsubscribe<PlayerConnectedToMatchmakingServerEvent, PlayerConnectedToMatchmakingServerAck>(Handle);
            return Task.CompletedTask;
        }

        #endregion

        #region Private Methods

        private Task<PlayerConnectedToMatchmakingServerAck> Handle(PlayerConnectedToMatchmakingServerEvent @event)
        {
            lock (_packetEncryptionLayer)
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
            }
            return Task.FromResult(new PlayerConnectedToMatchmakingServerAck());
        }
        #endregion
    }
}
