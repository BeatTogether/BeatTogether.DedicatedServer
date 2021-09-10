namespace BeatTogether.DedicatedServer.Kernel.Abstractions
{
    public interface ILobbyManager
    {
        bool AllPlayersReady { get; }
        bool SomePlayersReady { get; }
        bool NoPlayersReady { get; }

        void Update();
    }
}
