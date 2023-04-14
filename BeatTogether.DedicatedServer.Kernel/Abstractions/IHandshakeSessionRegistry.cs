using System.Net;
using BeatTogether.DedicatedServer.Kernel.Handshake;

namespace BeatTogether.DedicatedServer.Kernel.Abstractions
{
    public interface IHandshakeSessionRegistry
    {
        HandshakeSession GetOrAdd(EndPoint endPoint);
        HandshakeSession? TryGetByPlayerSessionId(string playerSessionId);
        void AddPendingPlayerSessionId(string playerSessionId);
        bool TryRemovePendingPlayerSessionId(string playerSessionId);
    }
}