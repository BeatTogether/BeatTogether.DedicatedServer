using Autobus;

namespace BeatTogether.DedicatedServer.Interface
{
    public class DedicatedServerServiceContract : BaseServiceContract
    {
        public override void Build(IServiceContractBuilder builder) =>
            builder
                .UseName("DedicatedServer")
                .AddInterface<IMatchmakingService>();
    }
}
