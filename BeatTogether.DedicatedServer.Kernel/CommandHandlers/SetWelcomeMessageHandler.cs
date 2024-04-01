using BeatTogether.DedicatedServer.Ignorance.IgnoranceCore;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.CommandHandlers;
using BeatTogether.DedicatedServer.Kernel.Configuration;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MPChatPackets;

namespace BeatTogether.DedicatedServer.Kernel.Commands.CommandHandlers
{
    class SetWelcomeMessageHandler : BaseCommandHandler<SetWelcomeMessage>
    {
        private readonly IPacketDispatcher _packetDisapatcher;
        private readonly InstanceConfiguration _Configuration;
        private readonly IDedicatedInstance _instance;

        public SetWelcomeMessageHandler(IPacketDispatcher packetDisapatcher, InstanceConfiguration instanceConfiguration, IDedicatedInstance instance)
        {
            _packetDisapatcher = packetDisapatcher;
            _Configuration = instanceConfiguration;
            _instance = instance;
        }

        public override void Handle(IPlayer player, SetWelcomeMessage command)
        {
            _Configuration.WelcomeMessage = command.Text;
            _packetDisapatcher.SendToPlayer(player, new MpcTextChatPacket
            {
                Text = "Server name is now: " + command.Text
            }, IgnoranceChannelTypes.Reliable);
        }
    }
}
