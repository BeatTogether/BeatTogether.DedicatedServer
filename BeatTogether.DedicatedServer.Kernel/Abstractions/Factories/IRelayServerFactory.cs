using System.Net;
using BeatTogether.DedicatedServer.Kernel.Implementations;

namespace BeatTogether.DedicatedServer.Kernel.Abstractions.Providers
{
    public interface IRelayServerFactory
    {
        RelayServer? GetRelayServer(IPEndPoint sourceEndPoint, IPEndPoint targetEndPoint);
    }
}
