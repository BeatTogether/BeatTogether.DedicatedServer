
namespace BeatTogether.DedicatedServer.Interface.Events
{
    public sealed record ServerInGameplayEvent(
        string Secret,
        bool InGame,
        string LevelID
        );
}
