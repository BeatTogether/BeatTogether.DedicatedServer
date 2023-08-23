using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.CommandHandlers;
using BeatTogether.DedicatedServer.Kernel.Configuration;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MPChatPackets;

namespace BeatTogether.DedicatedServer.Kernel.Commands.CommandHandlers
{
    class SetServerNameHandler : BaseCommandHandler<SetServerName>
    {
        private readonly IPacketDispatcher _packetDisapatcher;
        private readonly InstanceConfiguration _Configuration;
        private readonly IDedicatedInstance _instance;

        public SetServerNameHandler(IPacketDispatcher packetDisapatcher, InstanceConfiguration instanceConfiguration, IDedicatedInstance instance)
        {
            _packetDisapatcher = packetDisapatcher;
            _Configuration = instanceConfiguration;
            _instance = instance;
        }

        public override void Handle(IPlayer player, SetServerName command)
        {
            _Configuration.ServerName = command.Name;
            _instance.InstanceConfigUpdated();
            _packetDisapatcher.SendToNearbyPlayers(new MpcTextChatPacket
            {
                Text = "Server name is now: " + command.Name
            }, LiteNetLib.Enums.DeliveryMethod.ReliableOrdered);
        }
    }
}
