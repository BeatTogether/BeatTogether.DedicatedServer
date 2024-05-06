namespace BeatTogether.DedicatedServer.Instancing.Abstractions
{
    public interface IPortAllocator
    {
        int? AcquirePort();
        bool ReleasePort(int port);
    }
}
