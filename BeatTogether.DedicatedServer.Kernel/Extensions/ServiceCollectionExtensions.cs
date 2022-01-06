using BeatTogether.DedicatedServer.Kernel;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Configuration;
using BeatTogether.DedicatedServer.Kernel.Managers;
using BeatTogether.DedicatedServer.Kernel.Managers.Abstractions;
using BeatTogether.LiteNetLib;
using BeatTogether.LiteNetLib.Dispatchers;
using BeatTogether.LiteNetLib.Sources;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Reflection;

namespace BeatTogether.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDedicatedServer(this IServiceCollection services) =>
            services
                .AddDedicatedServerMessaging()
                .AddScoped<InstanceConfiguration>()
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
                .AddAllPacketHandlersFromAssembly(typeof(PacketSource).Assembly);

        public static IServiceCollection AddExisting<TService, TImplementation>(this IServiceCollection services)
            where TService : class
            where TImplementation : class, TService
            => services.AddScoped<TService, TImplementation>(services => services.GetRequiredService<TImplementation>());

        public static IServiceCollection AddAllPacketHandlersFromAssembly(this IServiceCollection services, Assembly assembly)
        {
            var genericInterface = typeof(IPacketHandler<>);
            var eventHandlerTypes = assembly
                .GetTypes()
                .Where(type => type.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == genericInterface));
            foreach (var eventHandlerType in eventHandlerTypes)
                if (!eventHandlerType.IsAbstract)
                    services.AddTransient(
                        genericInterface.MakeGenericType(eventHandlerType.BaseType!.GetGenericArguments()),
                        eventHandlerType);
            return services;
        }
    }
}
