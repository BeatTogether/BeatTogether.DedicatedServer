using BeatTogether.DedicatedServer.Kernel.Enums;
using BeatTogether.DedicatedServer.Messaging.Models;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.GameplayRpc;
using System.Threading;

namespace BeatTogether.DedicatedServer.Kernel.Abstractions
{
    public interface IGameplayManager
    {
        string SessionGameId { get; }
        GameplayManagerState State { get; }

        void HandleGameSceneLoaded(IPlayer player, SetGameplaySceneReadyPacket packet);
        void HandleGameSongLoaded(IPlayer player);
        void StartSong(BeatmapIdentifier beatmap, CancellationToken cancellationToken);
    }
}
