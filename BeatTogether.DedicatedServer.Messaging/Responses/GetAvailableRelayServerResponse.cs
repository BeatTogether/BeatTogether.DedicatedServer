using BeatTogether.DedicatedServer.Messaging.Abstractions;
using Obvs.Types;

namespace BeatTogether.DedicatedServer.Messaging.Responses
{
    public class GetAvailableRelayServerResponse : IDedicatedServerMessage, IResponse
    {
        public enum ErrorCode
        {
            None = 0,
            NoAvailableRelayServers = 1
        }

        public string RequestId { get; set; }
        public string RequesterId { get; set; }
        public ErrorCode Error { get; set; }
        public string RemoteEndPoint { get; set; }
    }
}
