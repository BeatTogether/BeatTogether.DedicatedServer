using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Configuration;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MPChatPackets;
using BeatTogether.LiteNetLib.Enums;
using Serilog;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.MPChat
{
    public class MpcCapabilitiesPacketHandler : BasePacketHandler<MpcCapabilitiesPacket>
    {
        private readonly IPacketDispatcher _packetDispatcher;
        private readonly InstanceConfiguration _instanceConfiguration;
        private readonly ILogger _logger = Log.ForContext<MpcCapabilitiesPacketHandler>();
        public MpcCapabilitiesPacketHandler(IPacketDispatcher packetDispatcher,
            InstanceConfiguration instanceConfiguration)
        {
            _packetDispatcher = packetDispatcher;
            _instanceConfiguration = instanceConfiguration;
        }

        public override void Handle(IPlayer sender, MpcCapabilitiesPacket packet)
        {
            bool FirstJoin = !sender.CanTextChat && packet.CanTextChat;
            sender.CanReceiveVoiceChat = packet.CanReceiveVoiceChat;
            sender.CanTransmitVoiceChat = packet.CanTransmitVoiceChat;
            sender.CanTextChat = packet.CanTextChat;
            if (FirstJoin)
            {
                _packetDispatcher.SendToPlayer(sender, new MpcTextChatPacket
                {
                    Text = "Welcome to " + _instanceConfiguration.ServerName + " Type /help to get a list of commands!"
                }, DeliveryMethod.ReliableOrdered);
                if (_instanceConfiguration.WelcomeMessage != string.Empty)
                    _packetDispatcher.SendToPlayer(sender, new MpcTextChatPacket
                    {
                        Text = _instanceConfiguration.WelcomeMessage
                    }, DeliveryMethod.ReliableOrdered);

            }
        }
    }
}
