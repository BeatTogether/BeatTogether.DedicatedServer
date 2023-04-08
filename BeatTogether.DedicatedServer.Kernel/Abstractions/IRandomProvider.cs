namespace BeatTogether.DedicatedServer.Kernel.Abstractions
{
    public interface IRandomProvider
    {
        byte[] GetRandom();
    }
}