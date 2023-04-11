using BeatTogether.Core.Messaging.Implementations.Registries;
using BeatTogether.Core.Messaging.Messages;
using BeatTogether.DedicatedServer.Messaging.Messages.Handshake;
using BeatTogether.MasterServer.Messaging.Enums;

namespace BeatTogether.DedicatedServer.Messaging.Registries.Unconnected
{
    public class HandshakeMessageRegistry : BaseMessageRegistry
    {
        public override uint MessageGroup => 3192347326U;

        public HandshakeMessageRegistry()
        {
            Register<ClientHelloRequest>(HandshakeMessageType.ClientHelloRequest);
            Register<HelloVerifyRequest>(HandshakeMessageType.HelloVerifyRequest);
            Register<ClientHelloWithCookieRequest>(HandshakeMessageType.ClientHelloWithCookieRequest);
            Register<ServerHelloRequest>(HandshakeMessageType.ServerHelloRequest);
            Register<ServerCertificateRequest>(HandshakeMessageType.ServerCertificateRequest);
            Register<ClientKeyExchangeRequest>(HandshakeMessageType.ClientKeyExchangeRequest);
            Register<ChangeCipherSpecRequest>(HandshakeMessageType.ChangeCipherSpecRequest);
            Register<AcknowledgeMessage>(HandshakeMessageType.MessageReceivedAcknowledge);
            Register<MultipartMessage>(HandshakeMessageType.MultipartMessage);
        }
    }
}
