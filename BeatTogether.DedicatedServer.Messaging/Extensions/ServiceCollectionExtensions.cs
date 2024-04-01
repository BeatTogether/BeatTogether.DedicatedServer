using BeatTogether.DedicatedServer.Messaging.Registries;
using BeatTogether.DedicatedServer.Messaging.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace BeatTogether.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDedicatedServerMessaging(this IServiceCollection services) =>
            services
                .AddSingleton<IPacketRegistry, PacketRegistry>();
    }
}
