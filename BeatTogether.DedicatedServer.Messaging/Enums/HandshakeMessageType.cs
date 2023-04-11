namespace BeatTogether.MasterServer.Messaging.Enums
{
    public enum HandshakeMessageType : uint
    {
        ClientHelloRequest,
        HelloVerifyRequest,
        ClientHelloWithCookieRequest,
        ServerHelloRequest,
        ServerCertificateRequest,
        ClientKeyExchangeRequest = 6,
        ChangeCipherSpecRequest,
        MessageReceivedAcknowledge,
        MultipartMessage
    }
}
