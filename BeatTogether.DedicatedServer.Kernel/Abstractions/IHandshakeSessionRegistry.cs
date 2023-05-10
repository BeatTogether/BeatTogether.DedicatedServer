using System.Diagnostics.CodeAnalysis;
using System.Net;
using BeatTogether.DedicatedServer.Kernel.Handshake;

namespace BeatTogether.DedicatedServer.Kernel.Abstractions
{
    public interface IHandshakeSessionRegistry
    {
        HandshakeSession GetOrAdd(EndPoint endPoint);
        HandshakeSession? TryGetByPlayerSessionId(string playerSessionId);
        void AddPendingPlayerSessionId(string playerSessionId);
        void AddExtraPlayerSessionData(string playerSessionId, string ClientVersion, byte Platform, string PlayerPlatformUserId);
        void RemoveExtraPlayerSessionData(string playerSessionId, out string ClientVersion, out byte Platform, out string PlayerPlatformUserId);
        bool TryRemovePendingPlayerSessionId(string playerSessionId);
    }
}