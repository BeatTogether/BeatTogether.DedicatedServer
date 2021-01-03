using BeatTogether.DedicatedServer.Messaging.Abstractions;
using Obvs.Types;

namespace BeatTogether.DedicatedServer.Messaging.Requests
{
    public class GetAvailableRelayServerRequest : IDedicatedServerMessage, IRequest
    {
        public string RequestId { get; set; }
        public string RequesterId { get; set; }
        public string SourceEndPoint { get; set; }
        public string TargetEndPoint { get; set; }
    }
}
