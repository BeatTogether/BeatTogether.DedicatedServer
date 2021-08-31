using BeatTogether.DedicatedServer.Kernel.Abstractions;
using System.Threading;

namespace BeatTogether.DedicatedServer.Kernel
{
    public sealed class ServerContextAccessor : IServerContextAccessor
    {
        private static AsyncLocal<IServerContext> _context = new();

        public IServerContext Context 
        { 
            get => _context.Value!; 
            set => _context.Value = value; 
        }
    }
}
