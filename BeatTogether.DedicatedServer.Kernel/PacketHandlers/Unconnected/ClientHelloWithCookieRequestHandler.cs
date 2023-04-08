using System.Threading.Tasks;
using BeatTogether.Core.Messaging.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Handshake;
using BeatTogether.DedicatedServer.Messaging.Messages.Handshake;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.Unconnected
{
    public class ClientHelloWithCookieRequestHandler : BaseHandshakeMessageHandler<ClientHelloWithCookieRequest>
    {
        private IHandshakeService _handshakeService;

        public ClientHelloWithCookieRequestHandler(IHandshakeService handshakeService)
        {
            _handshakeService = handshakeService;
        }

        public override async Task<IMessage?> Handle(HandshakeSession session, ClientHelloWithCookieRequest message) =>
            await _handshakeService.ClientHelloWithCookie(session, message);
    }
}