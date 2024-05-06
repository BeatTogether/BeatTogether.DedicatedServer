using BeatTogether.DedicatedServer.Instancing.Configuration;
using BeatTogether.DedicatedServer.Instancing.Abstractions;
using BeatTogether.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BeatTogether.DedicatedServer.Kernel.Extensions;
using BeatTogether.Core.Abstractions;

namespace BeatTogether.DedicatedServer.Instancing.Extensions
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder UseDedicatedServerInstancing(this IHostBuilder hostBuilder) =>
            hostBuilder
                .ConfigureAppConfiguration()
                .UseSerilog()
                .UseDedicatedInstances()
                .ConfigureServices((hostBuilderContext, services) =>
                    services
                        .AddConfiguration<InstancingConfiguration>("Instancing")
                        .AddSingleton<IPortAllocator, PortAllocator>()
                        .AddSingleton<IInstanceRegistry, InstanceRegistry>()
                        .AddSingleton<IInstanceFactory, InstanceFactory>()
                        .AddSingleton<ILayer2, LayerService>()
                );
    }
}