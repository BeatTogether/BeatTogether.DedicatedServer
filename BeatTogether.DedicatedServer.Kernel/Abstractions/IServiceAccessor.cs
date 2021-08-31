namespace BeatTogether.DedicatedServer.Kernel.Abstractions
{
    public interface IServiceAccessor<TService>
    {
        TService Service { get; }

        TService Create<ServiceType>() where ServiceType : TService;
    }
}
