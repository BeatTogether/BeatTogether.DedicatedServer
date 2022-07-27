using BeatTogether.DedicatedServer.Interface.Enums;

namespace BeatTogether.DedicatedServer.Interface.Events
{
    public sealed record UpdateStatusEvent(
        string Secret,
        CountdownState CountdownState,
        MultiplayerGameState GameState,
        GameplayState GameplayState
        );
}
