using Autobus;
using BeatTogether.Core.ServerMessaging;
using BeatTogether.DedicatedServer.Interface;
using BeatTogether.DedicatedServer.Node.Configuration;
using BeatTogether.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BeatTogether.DedicatedServer.Node.Extensions
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder UseDedicatedServerNode(this IHostBuilder hostBuilder) =>
            hostBuilder
                .ConfigureAppConfiguration()
                .UseSerilog()
                .UseAutobus()
                .ConfigureServices((hostBuilderContext, services) =>
                    services
                        .AddConfiguration<NodeConfiguration>("ServerConfiguration")
                        .AddServiceKernel<IMatchmakingService, NodeMatchmakingService>()
                        .AddSingleton<ILayer1, ForwardServerEventsLayer>()
                        .AddHostedService<NodeMessageEventHandler>()
                );
    }
}