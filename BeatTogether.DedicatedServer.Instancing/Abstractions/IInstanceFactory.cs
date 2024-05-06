using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.Core.Abstractions;

namespace BeatTogether.DedicatedServer.Instancing.Abstractions
{
    public interface IInstanceFactory
    {
        public IDedicatedInstance? CreateInstance(
            IServerInstance serverInstance);
    }
}
