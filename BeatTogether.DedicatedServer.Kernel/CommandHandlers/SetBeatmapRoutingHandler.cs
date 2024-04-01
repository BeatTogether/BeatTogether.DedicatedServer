using BeatTogether.DedicatedServer.Ignorance.IgnoranceCore;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.CommandHandlers;
using BeatTogether.DedicatedServer.Kernel.Configuration;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MPChatPackets;

namespace BeatTogether.DedicatedServer.Kernel.Commands.CommandHandlers
{
    class SetNoteRoutingHandler : BaseCommandHandler<SetNoteRouting>
    {
        private readonly IPacketDispatcher _packetDisapatcher;
        private readonly InstanceConfiguration _Configuration;

        public SetNoteRoutingHandler(IPacketDispatcher packetDisapatcher, InstanceConfiguration instanceConfiguration)
        {
            _packetDisapatcher = packetDisapatcher;
            _Configuration = instanceConfiguration;
        }

        public override void Handle(IPlayer player, SetNoteRouting command)
        {
            _Configuration.DisableNotes = !command.Enabled;
            _packetDisapatcher.SendToNearbyPlayers(new MpcTextChatPacket
            {
                Text = "Beatmap notes: " + command.Enabled
            }, IgnoranceChannelTypes.Reliable);
        }
    }
}
