using System.Threading.Tasks;
using BeatTogether.Core.Messaging.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Handshake;
using BeatTogether.DedicatedServer.Messaging.Messages.GameLift;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.Unconnected
{
    public class AuthenticateGameLiftUserRequestHandler : BaseHandshakeMessageHandler<AuthenticateGameLiftUserRequest>
    {
        private readonly IHandshakeService _handshakeService;

        public AuthenticateGameLiftUserRequestHandler(IHandshakeService handshakeService)
        {
            _handshakeService = handshakeService;
        }

        public override async Task<IMessage?> Handle(HandshakeSession session, AuthenticateGameLiftUserRequest message)
            => await _handshakeService.AuthenticateGameLiftUser(session, message);
    }
}