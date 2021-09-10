namespace BeatTogether.DedicatedServer.Kernel.Abstractions
{
    public interface IPortAllocator
    {
        int? AcquirePort();
        bool ReleasePort(int port);
    }
}
