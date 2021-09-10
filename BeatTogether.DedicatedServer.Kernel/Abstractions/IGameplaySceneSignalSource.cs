using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.GameplayRpc;
using System.Threading;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Kernel.Abstractions
{
    public interface IGameplaySceneSignalSource
    {
        Task<SetGameplaySceneReadyPacket> WaitForSceneReady(CancellationToken cancellationToken);
        void SignalSceneReady(SetGameplaySceneReadyPacket packet);
    }
}
