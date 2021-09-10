using System.Threading;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Kernel.Abstractions
{
    public interface IGameplaySongSignalSource
    {
        Task WaitForSongReady(CancellationToken cancellationToken);
        void SignalSongReady();
    }
}
