using BeatTogether.DedicatedServer.Messaging.Models;

namespace BeatTogether.DedicatedServer.Kernel.Managers.Abstractions
{
    public interface ILobbyManager
    {
        bool AllPlayersReady { get; }
        bool SomePlayersReady { get; }
        bool NoPlayersReady { get; }
		bool AllPlayersSpectating { get; }

        void Update();
    }
}
