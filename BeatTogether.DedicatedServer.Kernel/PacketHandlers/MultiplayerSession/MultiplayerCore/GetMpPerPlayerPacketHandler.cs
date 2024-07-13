using BeatTogether.DedicatedServer.Ignorance.IgnoranceCore;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Configuration;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MpCorePackets;
using Serilog;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.MenuRpc
{
    class GetMpPerPlayerPacketHandler : BasePacketHandler<GetMpPerPlayerPacket>
    {
        private readonly InstanceConfiguration _configuration;
        private readonly IPacketDispatcher _PacketDispatcher;
        private readonly ILogger _logger = Log.ForContext<GetMpPerPlayerPacketHandler>();

        public GetMpPerPlayerPacketHandler(
            IPacketDispatcher PacketDispatcher,
            InstanceConfiguration configuration)
        {
            _PacketDispatcher = PacketDispatcher;
            _configuration = configuration;
        }

        public override void Handle(IPlayer sender, GetMpPerPlayerPacket packet)
        {

            _logger.Debug(
                $"Handling packet of type '{nameof(GetMpPerPlayerPacket)}' " +
                $"(SenderId={sender.ConnectionId})."
            );
            _PacketDispatcher.SendToPlayer(sender, new MpPerPlayerPacket()
            {
                PPDEnabled = _configuration.AllowPerPlayerDifficulties,
                PPMEnabled = _configuration.AllowPerPlayerModifiers,
            }, IgnoranceChannelTypes.Reliable);
        }
    }
}