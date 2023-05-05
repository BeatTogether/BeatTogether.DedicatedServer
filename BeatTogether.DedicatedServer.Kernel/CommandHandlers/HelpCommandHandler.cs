using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.CommandHandlers;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MPChatPackets;
using System.Text;

namespace BeatTogether.DedicatedServer.Kernel.Commands.CommandHandlers
{
    class HelpCommandHandler : BaseCommandHandler<HelpCommand>
    {
        private readonly ITextCommandRepository _commandRepository;
        private readonly IPacketDispatcher _packetDisapatcher;

        public HelpCommandHandler(ITextCommandRepository commandRepository, IPacketDispatcher packetDisapatcher)
        {
            _commandRepository = commandRepository;
            _packetDisapatcher = packetDisapatcher;
        }

        public override void Handle(IPlayer player, HelpCommand command)
        {
            if (command.SpecificCommandName != null)
            { 
                if (!_commandRepository.GetCommand(command.SpecificCommandName, player.GetAccessLevel(), out var textCommand))
                {
                    _packetDisapatcher.SendToPlayer(player, new MpcTextChatPacket
                    {
                        Text = "Command you searched help for does not exist or is above your access level"
                    }, LiteNetLib.Enums.DeliveryMethod.ReliableOrdered);
                }
                _packetDisapatcher.SendToPlayer(player, new MpcTextChatPacket
                {
                    Text = textCommand.CommandName + "  :  " + textCommand.Description + "  :  ShortHand: " + textCommand.ShortHandName
                }, LiteNetLib.Enums.DeliveryMethod.ReliableOrdered);
                return;
            }
            string[] CommandList = _commandRepository.GetTextCommandNames(player.GetAccessLevel());
            StringBuilder Response = new();
            Response.Append("Command List: ");
            for (int i = 0; i < CommandList.Length; i++)
            {
                Response.Append("/" + CommandList[i] + "  :  ");
            }
            _packetDisapatcher.SendToPlayer(player, new MpcTextChatPacket
            {
                Text = Response.ToString()
            }, LiteNetLib.Enums.DeliveryMethod.ReliableOrdered);
        }
    }
}
