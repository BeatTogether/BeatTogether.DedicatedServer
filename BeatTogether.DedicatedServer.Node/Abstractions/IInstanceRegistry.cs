using BeatTogether.DedicatedServer.Kernel.Abstractions;
using System.Diagnostics.CodeAnalysis;

namespace BeatTogether.DedicatedServer.Node.Abstractions
{
    public interface IInstanceRegistry
    {
        public bool AddInstance(IDedicatedInstance instance);
        public bool RemoveInstance(IDedicatedInstance instance);
        public IDedicatedInstance GetInstance(string secret);
        public bool TryGetInstance(string secret, [MaybeNullWhen(false)] out IDedicatedInstance instance);


        public string[] ListPublicInstanceSecrets();
        public int GetPublicServerCount();
        public int GetServerCount();
    }
}
