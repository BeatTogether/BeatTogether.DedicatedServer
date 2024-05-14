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
        long GetMillisBetweenPoseSyncStateDeltaPackets();
        long GetMillisBetweenScoreSyncStateDeltaPackets();

        public void AddExtraPlayerSessionData(Core.Abstractions.IPlayer playerSessionData);
        public bool RemoveExtraPlayerSessionDataAndApply(Core.Abstractions.IPlayer playerSessionData/*out string ClientVersion, out byte Platform, out string PlayerPlatformUserId*/);
        public bool RemoveExtraPlayerSessionData(string playerSessionId/*out string ClientVersion, out byte Platform, out string PlayerPlatformUserId*/);
    }
}
