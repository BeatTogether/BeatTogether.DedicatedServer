namespace BeatTogether.DedicatedServer.Kernel.Abstractions
{
    public interface IServiceAccessor<IService>
    {
        IService Service { get; }

        IService Create();
        IService Bind(IService service);
    }
}
