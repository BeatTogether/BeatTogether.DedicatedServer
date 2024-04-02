using System.Security.Cryptography;
using Autobus;
using BeatTogether.DedicatedServer.Interface;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Extensions;
using BeatTogether.DedicatedServer.Kernel.Providers;
using BeatTogether.DedicatedServer.Node.Abstractions;
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
                .UseDedicatedInstances()
                .ConfigureServices((hostBuilderContext, services) =>
                    services
                        .AddCoreSecurity()
                        .AddConfiguration<NodeConfiguration>("Node")
                        .AddSingleton(RandomNumberGenerator.Create())
                        .AddSingleton<ICookieProvider, CookieProvider>()
                        .AddSingleton<IRandomProvider, RandomProvider>()
                        .AddSingleton<IPortAllocator, PortAllocator>()
                        .AddSingleton<IInstanceRegistry, InstanceRegistry>()
                        .AddSingleton<IInstanceFactory, InstanceFactory>()
                        .AddServiceKernel<IMatchmakingService, NodeService>()
                        .AddHostedService<MasterServerEventHandler>()
                );
    }
}