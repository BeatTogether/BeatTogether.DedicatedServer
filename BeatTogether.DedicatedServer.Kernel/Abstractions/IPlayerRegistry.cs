using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace BeatTogether.DedicatedServer.Kernel.Abstractions
{
    public interface IPlayerRegistry
    {
        IPlayer[] Players { get; }
        bool AddPlayer(IPlayer player);
        void RemovePlayer(IPlayer player);
        int GetPlayerCount();
        bool TryGetPlayer(EndPoint remoteEndPoint, [MaybeNullWhen(false)] out IPlayer player);
        bool TryGetPlayer(byte connectionId, [MaybeNullWhen(false)] out IPlayer player);
        bool TryGetPlayer(string userId, [MaybeNullWhen(false)] out IPlayer player);
        int GetMillisBetweenSyncStatePackets();

        public void AddExtraPlayerSessionData(string playerSessionId, string ClientVersion, byte PlatformId, string PlayerPlatformUserId);
        public bool RemoveExtraPlayerSessionData(string playerSessionId, out string ClientVersion, out byte Platform, out string PlayerPlatformUserId);
    }
}
