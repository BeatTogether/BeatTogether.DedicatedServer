namespace BeatTogether.DedicatedServer.Kernel.Abstractions
{
    public interface IServerContextAccessor
    {
        IServerContext Context { get; set; }
    }
}
