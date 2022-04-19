using Autobus;
using BeatTogether.DedicatedServer.Interface.Events;

namespace BeatTogether.DedicatedServer.Interface
{
    public class DedicatedServerServiceContract : BaseServiceContract
    {
        public override void Build(IServiceContractBuilder builder) =>
            builder
                .UseName("DedicatedServer")
                .AddInterface<IMatchmakingService>()
                .AddEvent<MatchmakingServerStartedEvent>()
                .AddEvent<MatchmakingServerStoppedEvent>()
                .AddEvent<FromServerCreateServerEvent>();

    }
}
