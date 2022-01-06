namespace BeatTogether.DedicatedServer.Node.Abstractions
{
    public interface IPortAllocator
    {
        int? AcquirePort();
        bool ReleasePort(int port);
    }
}
