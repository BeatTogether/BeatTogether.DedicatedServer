namespace BeatTogether.DedicatedServer.Kernel.Abstractions
{
    public interface IAsyncLocalServiceAccessor<TService>
    {
        TService Service { get; }

        TService Set(TService service);
    }
}
