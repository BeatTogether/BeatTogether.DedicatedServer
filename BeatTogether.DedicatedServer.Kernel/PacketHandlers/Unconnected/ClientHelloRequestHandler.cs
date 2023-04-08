using System.Threading.Tasks;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Handshake;
using BeatTogether.DedicatedServer.Messaging.Messages.Handshake;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.Unconnected
{
    public class ClientHelloRequestHandler : BaseHandshakeMessageHandler<ClientHelloRequest>
    {
        private IHandshakeService _handshakeService;
        private IUnconnectedDispatcher _unconnectedDispatcher;

        public ClientHelloRequestHandler(IHandshakeService handshakeService, IUnconnectedDispatcher unconnectedDispatcher)
        {
            _handshakeService = handshakeService;
            _unconnectedDispatcher = unconnectedDispatcher;
        }
        
        public override async Task Handle(HandshakeSession session, ClientHelloRequest message)
        {
            var request = await _handshakeService.ClientHello(session, message);
            _unconnectedDispatcher.Send(session, request);
        }
    }
}