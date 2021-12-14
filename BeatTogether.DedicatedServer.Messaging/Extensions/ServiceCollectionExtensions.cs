using BeatTogether.DedicatedServer.Messaging;
using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.DedicatedServer.Messaging.PacketRegistries;
using BeatTogether.LiteNetLib.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace BeatTogether.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDedicatedServerMessaging(this IServiceCollection services) =>
            services
                .AddSingleton<IPacketRegistry, PacketRegistry>()
                .AddSingleton<DedicatedServer.Messaging.Abstractions.IPacketReader, PacketReader>()
                .AddSingleton<IPacketWriter, PacketWriter>();
    }
}
