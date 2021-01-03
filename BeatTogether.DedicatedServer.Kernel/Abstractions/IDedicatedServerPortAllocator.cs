namespace BeatTogether.DedicatedServer.Kernel.Abstractions
{
    public interface IDedicatedServerPortAllocator
    {
        int? AcquireRelayServerPort();
        bool ReleaseRelayServerPort(int port);
    }
}
