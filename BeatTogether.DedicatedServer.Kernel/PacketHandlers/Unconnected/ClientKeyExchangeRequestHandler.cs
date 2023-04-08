using System.Threading.Tasks;
using BeatTogether.Core.Messaging.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Handshake;
using BeatTogether.DedicatedServer.Messaging.Messages.Handshake;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.Unconnected
{
    public class ClientKeyExchangeRequestHandler : BaseHandshakeMessageHandler<ClientKeyExchangeRequest>
    {
        private readonly IHandshakeService _handshakeService;

        public ClientKeyExchangeRequestHandler(IHandshakeService handshakeService)
        {
            _handshakeService = handshakeService;
        }

        public override async Task<IMessage?> Handle(HandshakeSession session, ClientKeyExchangeRequest message) =>
            await _handshakeService.ClientKeyExchange(session, message);
    }
}