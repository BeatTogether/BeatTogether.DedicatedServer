using System.Threading.Tasks;
using BeatTogether.Core.Messaging.Abstractions;
using BeatTogether.Core.Messaging.Messages;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Handshake;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.Unconnected
{
    public class AcknowledgeMessageHandler : BaseHandshakeMessageHandler<AcknowledgeMessage>
    {
        private IUnconnectedDispatcher _unconnectedDispatcher;

        public AcknowledgeMessageHandler(IUnconnectedDispatcher unconnectedDispatcher)
        {
            _unconnectedDispatcher = unconnectedDispatcher;
        }

        public override Task<IMessage?> Handle(HandshakeSession session, AcknowledgeMessage message)
        {
            _unconnectedDispatcher.Acknowledge(session, message.ResponseId);
            return Task.FromResult(default(IMessage?));
        }
            
    }
}