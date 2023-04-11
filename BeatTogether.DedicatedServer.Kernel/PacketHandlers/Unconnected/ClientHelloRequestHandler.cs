using System.Threading.Tasks;
using BeatTogether.Core.Messaging.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Handshake;
using BeatTogether.DedicatedServer.Messaging.Messages.Handshake;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.Unconnected
{
    public class ClientHelloRequestHandler : BaseHandshakeMessageHandler<ClientHelloRequest>
    {
        private readonly IHandshakeService _handshakeService;

        public ClientHelloRequestHandler(IHandshakeService handshakeService)
        {
            _handshakeService = handshakeService;
        }
        
        public override async Task<IMessage?> Handle(HandshakeSession session, ClientHelloRequest message) => 
            await _handshakeService.ClientHello(session, message);
    }
}