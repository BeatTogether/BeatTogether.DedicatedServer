using BeatTogether.DedicatedServer.Ignorance.IgnoranceCore;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.CommandHandlers;
using BeatTogether.DedicatedServer.Kernel.Configuration;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MPChatPackets;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MpCorePackets;

namespace BeatTogether.DedicatedServer.Kernel.Commands.CommandHandlers
{
    class SetPerPlayerDiffHandler : BaseCommandHandler<SetPerPlayerDifficulties>
    {
        private readonly IPacketDispatcher _packetDisapatcher;
        private readonly InstanceConfiguration _Configuration;

        public SetPerPlayerDiffHandler(IPacketDispatcher packetDisapatcher, InstanceConfiguration instanceConfiguration)
        {
            _packetDisapatcher = packetDisapatcher;
            _Configuration = instanceConfiguration;
        }

        public override void Handle(IPlayer player, SetPerPlayerDifficulties command)
        {
            _Configuration.AllowPerPlayerDifficulties = command.Enabled;
            _packetDisapatcher.SendToNearbyPlayers(new MpcTextChatPacket
            {
                Text = "Per player difficulties: " + command.Enabled
            }, IgnoranceChannelTypes.Reliable);
            _packetDisapatcher.SendToNearbyPlayers(new PerPlayer()
            {
                PPDEnabled = _Configuration.AllowPerPlayerDifficulties,
                PPMEnabled = _Configuration.AllowPerPlayerModifiers,
            }, IgnoranceChannelTypes.Reliable);
        }
    }
}
