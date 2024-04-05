using BeatTogether.DedicatedServer.Ignorance.IgnoranceCore;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Configuration;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MpCorePackets;
using Serilog;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.MenuRpc
{
    class PerPlayerHandler : BasePacketHandler<PerPlayer>
    {
        private readonly InstanceConfiguration _configuration;
        private readonly IPacketDispatcher _PacketDispatcher;
        private readonly ILogger _logger = Log.ForContext<GetPerPlayerHandler>();

        public PerPlayerHandler(
            IPacketDispatcher PacketDispatcher,
            InstanceConfiguration configuration)
        {
            _PacketDispatcher = PacketDispatcher;
            _configuration = configuration;
        }

        public override void Handle(IPlayer sender, PerPlayer packet)
        {

            _logger.Debug(
                $"Handling packet of type '{nameof(PerPlayer)}' " +
                $"(SenderId={sender.ConnectionId})."
            );
            if(sender.IsServerOwner)
            {
                _configuration.AllowPerPlayerDifficulties = packet.PPDEnabled;
                _configuration.AllowPerPlayerModifiers = packet.PPMEnabled;
                _PacketDispatcher.SendToNearbyPlayers(new PerPlayer()
                {
                    PPDEnabled = _configuration.AllowPerPlayerDifficulties,
                    PPMEnabled = _configuration.AllowPerPlayerModifiers,
                }, IgnoranceChannelTypes.Reliable);
            }

        }
    }
}