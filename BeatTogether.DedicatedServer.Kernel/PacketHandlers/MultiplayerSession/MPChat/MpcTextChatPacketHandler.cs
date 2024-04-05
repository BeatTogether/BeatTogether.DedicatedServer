using BeatTogether.DedicatedServer.Ignorance.IgnoranceCore;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.CommandHandlers;
using BeatTogether.DedicatedServer.Kernel.Configuration;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MPChatPackets;
using Serilog;
using System;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.MPChat
{
    public class MpcTextChatPacketHandler : BasePacketHandler<MpcTextChatPacket>
    {
        private readonly IPacketDispatcher _packetDispatcher;
        private readonly InstanceConfiguration _instanceConfiguration;
        private readonly ITextCommandRepository _CommandRepository;
        private readonly IServiceProvider _serviceProvider;
        //private readonly ILogger _logger = Log.ForContext<MpcTextChatPacketHandler>();


        public MpcTextChatPacketHandler(IPacketDispatcher packetDispatcher,
            ITextCommandRepository textCommandRepository,
            InstanceConfiguration instanceConfiguration,
            IServiceProvider serviceProvider)
        {
            _packetDispatcher = packetDispatcher;
            _instanceConfiguration = instanceConfiguration;
            _CommandRepository = textCommandRepository;
            _serviceProvider = serviceProvider;
        }

        public override void Handle(IPlayer sender, MpcTextChatPacket packet)
        {
            if (packet.Text.Length < _instanceConfiguration.MaxLengthCommand && packet.Text.StartsWith("/")){
                string[] CommandValues = packet.Text[1..].Split(' ');
                if (_CommandRepository.GetCommand(CommandValues, sender.GetAccessLevel(), out var TextCommand))
                {
                    var commandType = TextCommand.GetType();
                    var packetHandlerType = typeof(ICommandHandler<>)
                        .MakeGenericType(commandType);
                    var Command = _serviceProvider.GetService(packetHandlerType);
                    if (Command != null)
                        ((ICommandHandler)Command).Handle(sender, TextCommand);
                    return;
                }
                _packetDispatcher.SendToPlayer(sender, new MpcTextChatPacket
                {
                    Text = "Command not found or too long"
                }, IgnoranceChannelTypes.Reliable);
            }
        }
    }
}
