using BeatTogether.Core.Enums;

namespace BeatTogether.DedicatedServer.Interface.Events
{
    public sealed record ServerInGameplayEvent(
        string Secret,
        MultiplayerGameState MultiplayerGameState
        );
}
