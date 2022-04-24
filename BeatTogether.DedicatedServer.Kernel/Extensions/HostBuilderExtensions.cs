using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Configuration;
using BeatTogether.DedicatedServer.Kernel.Managers;
using BeatTogether.DedicatedServer.Kernel.Managers.Abstractions;
using BeatTogether.Extensions;
using BeatTogether.LiteNetLib;
using BeatTogether.LiteNetLib.Configuration;
using BeatTogether.LiteNetLib.Dispatchers;
using BeatTogether.LiteNetLib.Extensions;
using BeatTogether.LiteNetLib.Sources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BeatTogether.DedicatedServer.Kernel.Extensions
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder UseDedicatedInstances(this IHostBuilder hostBuilder) =>
            hostBuilder
                .ConfigureAppConfiguration()
                .UseSerilog()
                .ConfigureServices((hostBuilderContext, services) =>
                    services
                        .AddLiteNetMessaging()
                        .AddConfiguration<LiteNetConfiguration>("LiteNetLib")
                        .AddScoped<InstanceConfiguration>()
                        .AddDedicatedServerMessaging()
                        .AddScoped<DedicatedInstance>()
                        .AddExisting<IDedicatedInstance, DedicatedInstance>()
                        .AddExisting<LiteNetServer, DedicatedInstance>()
                        .AddScoped<IPlayerRegistry, PlayerRegistry>()
                        .AddScoped<ConnectedMessageSource, PacketSource>()
                        .AddScoped<PacketDispatcher>()
                        .AddExisting<IPacketDispatcher, PacketDispatcher>()
                        .AddExisting<ConnectedMessageDispatcher, PacketDispatcher>()
                        .AddScoped<ILobbyManager, LobbyManager>()
                        .AddScoped<IGameplayManager, GameplayManager>()
                        .AddScoped<IRequirementCheck, RequirementCheck>()
                        .AddAllPacketHandlersFromAssembly(typeof(PacketSource).Assembly)
                );
    }
}
