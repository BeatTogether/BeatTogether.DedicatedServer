using BeatTogether.DedicatedServer.Ignorance.IgnoranceCore;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Configuration;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MpCorePackets;
using Serilog;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.MenuRpc
{
    class MpPerPlayerPacketHandler : BasePacketHandler<MpPerPlayerPacket>
    {
        private readonly InstanceConfiguration _configuration;
        private readonly IPacketDispatcher _PacketDispatcher;
        private readonly ILogger _logger = Log.ForContext<GetMpPerPlayerPacketHandler>();

        public MpPerPlayerPacketHandler(
            IPacketDispatcher PacketDispatcher,
            InstanceConfiguration configuration)
        {
            _PacketDispatcher = PacketDispatcher;
            _configuration = configuration;
        }

        public override void Handle(IPlayer sender, MpPerPlayerPacket packet)
        {

            _logger.Debug(
                $"Handling packet of type '{nameof(MpPerPlayerPacket)}' " +
                $"(SenderId={sender.ConnectionId})" +
                $"(PPDEnabled={packet.PPDEnabled}, PPMEnabled={packet.PPMEnabled})."
            );
            if(sender.IsServerOwner)
            {
                _configuration.AllowPerPlayerDifficulties = packet.PPDEnabled;
                _configuration.AllowPerPlayerModifiers = packet.PPMEnabled;
                _PacketDispatcher.SendToNearbyPlayers(new MpPerPlayerPacket()
                {
                    PPDEnabled = _configuration.AllowPerPlayerDifficulties,
                    PPMEnabled = _configuration.AllowPerPlayerModifiers,
                }, IgnoranceChannelTypes.Reliable);
            }

        }
    }
}