using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.GameplayRpc;
using System.Threading;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Kernel.Abstractions
{
    public interface IGameplayFinishedSignalSource
    {
        Task<LevelFinishedPacket> WaitForLevelFinished(CancellationToken cancellationToken);
        void SignalLevelFinished(LevelFinishedPacket packet);
    }
}
