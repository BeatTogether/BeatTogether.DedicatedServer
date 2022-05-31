using BeatTogether.DedicatedServer.Interface.Enums;

namespace BeatTogether.DedicatedServer.Interface.Models
{
    public record AdvancedPlayer(
        SimplePlayer SimplePlayer,
        byte ConnectionId,
        bool IsManager,
        bool IsPlayer,
        bool IsSpectating,
        bool WantsToPlayNextLevel,
        bool IsBackgrounded,
        bool InGameplay,
        bool WasActiveAtLevelStart,
        bool IsActive,
        bool FinishedLevel,
        bool InMenu,
        bool IsModded,
        bool InLobby);
}
