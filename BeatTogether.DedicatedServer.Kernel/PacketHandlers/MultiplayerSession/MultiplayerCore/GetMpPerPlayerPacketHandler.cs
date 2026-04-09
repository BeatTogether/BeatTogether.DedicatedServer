using BeatTogether.DedicatedServer.Ignorance.IgnoranceCore;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Configuration;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MpCorePackets;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.MenuRpc
{
    class GetMpPerPlayerPacketHandler : BasePacketHandler<GetMpPerPlayerPacket>
    {
        private readonly InstanceConfiguration _configuration;
        private readonly IPacketDispatcher _PacketDispatcher;
        private readonly Internal_Logger _logger;

        public GetMpPerPlayerPacketHandler(
            IPacketDispatcher PacketDispatcher,
            InstanceConfiguration configuration,
            Kernel_Logger kernel_Logger)
        {
            _PacketDispatcher = PacketDispatcher;
            _configuration = configuration;
            _logger = kernel_Logger.ForContext<GetMpPerPlayerPacketHandler>();
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