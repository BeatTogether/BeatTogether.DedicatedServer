namespace BeatTogether.DedicatedServer.Interface.Events
{
    public sealed record UpdatePlayersEvent(
        string Secret,
        string[] HashedUserIds);
}
