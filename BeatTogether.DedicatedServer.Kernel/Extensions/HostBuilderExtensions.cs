using Autobus;
using BeatTogether.DedicatedServer.Interface;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Abstractions.Providers;
using BeatTogether.DedicatedServer.Kernel.Configuration;
using BeatTogether.DedicatedServer.Kernel.Implementations;
using BeatTogether.DedicatedServer.Kernel.Implementations.Factories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BeatTogether.Extensions
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder UseDedicatedServerKernel(this IHostBuilder hostBuilder) =>
            hostBuilder
                .ConfigureAppConfiguration()
                .UseSerilog()
                .UseAutobus()
                .ConfigureServices((hostBuilderContext, services) =>
                    services
                        .AddConfiguration<RelayServerConfiguration>("RelayServers")
                        .AddSingleton<IDedicatedServerPortAllocator, DedicatedServerPortAllocator>()
                        .AddSingleton<IRelayServerFactory, RelayServerFactory>()
                        .AddServiceKernel<IRelayServerService, RelayServerService>()
                );
    }
}
