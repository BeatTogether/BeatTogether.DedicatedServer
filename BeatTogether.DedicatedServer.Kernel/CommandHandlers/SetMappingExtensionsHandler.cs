using BeatTogether.DedicatedServer.Ignorance.IgnoranceCore;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.CommandHandlers;
using BeatTogether.DedicatedServer.Kernel.Configuration;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MPChatPackets;

namespace BeatTogether.DedicatedServer.Kernel.Commands.CommandHandlers
{
    class SetMappingExtensionsHandler : BaseCommandHandler<SetMappingExtensions>
    {
        private readonly IPacketDispatcher _packetDisapatcher;
        private readonly InstanceConfiguration _Configuration;

        public SetMappingExtensionsHandler(IPacketDispatcher packetDisapatcher, InstanceConfiguration instanceConfiguration)
        {
            _packetDisapatcher = packetDisapatcher;
            _Configuration = instanceConfiguration;
        }

        public override void Handle(IPlayer player, SetMappingExtensions command)
        {
            _Configuration.AllowMappingExtensions = command.Enabled;
            _packetDisapatcher.SendToNearbyPlayers(new MpcTextChatPacket
            {
                Text = "Mapping Extensions: " + command.Enabled
            }, IgnoranceChannelTypes.Reliable);
        }
    }
}
