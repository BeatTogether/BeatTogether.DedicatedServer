using System.Threading.Tasks;
using BeatTogether.DedicatedServer.Kernel.Handshake;
using BeatTogether.DedicatedServer.Messaging.Messages.GameLift;
using BeatTogether.DedicatedServer.Messaging.Messages.Handshake;

namespace BeatTogether.DedicatedServer.Kernel.Abstractions
{
    public interface IHandshakeService
    {
        Task<HelloVerifyRequest> ClientHello(HandshakeSession session, ClientHelloRequest request);
        Task<ServerHelloRequest> ClientHelloWithCookie(HandshakeSession session, ClientHelloWithCookieRequest request);
        Task<ChangeCipherSpecRequest> ClientKeyExchange(HandshakeSession session, ClientKeyExchangeRequest request);
        Task<AuthenticateGameLiftUserResponse> AuthenticateGameLiftUser(HandshakeSession session, AuthenticateGameLiftUserRequest request);
    }
}