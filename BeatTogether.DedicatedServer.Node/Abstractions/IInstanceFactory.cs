using BeatTogether.DedicatedServer.Interface.Models;
using BeatTogether.DedicatedServer.Kernel.Abstractions;

namespace BeatTogether.DedicatedServer.Node.Abstractions
{
    public interface IInstanceFactory
    {
        public IDedicatedInstance? CreateInstance(string secret, string managerId, GameplayServerConfiguration config);
    }
}
