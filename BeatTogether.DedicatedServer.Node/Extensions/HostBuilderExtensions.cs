using Autobus;
using BeatTogether.DedicatedServer.Interface;
using BeatTogether.DedicatedServer.Kernel;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Encryption;
using BeatTogether.DedicatedServer.Kernel.Encryption.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Extensions;
using BeatTogether.DedicatedServer.Node.Abstractions;
using BeatTogether.DedicatedServer.Node.Configuration;
using BeatTogether.Extensions;
using BeatTogether.LiteNetLib.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Security.Cryptography;

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
                        .AddTransient<RNGCryptoServiceProvider>()
                        .AddSingleton<IEncryptedPacketReader, EncryptedPacketReader>()
                        .AddSingleton<IEncryptedPacketWriter, EncryptedPacketWriter>()
                        .AddSingleton<PacketEncryptionLayer>()
                        .AddSingleton<IPacketLayer, PacketEncryptionLayer>(services =>
                            services.GetRequiredService<PacketEncryptionLayer>())
                        .AddSingleton<IPortAllocator, PortAllocator>()
                        .AddSingleton<IInstanceRegistry, InstanceRegistry>()
                        .AddSingleton<IInstanceFactory, InstanceFactory>()
                        .AddSingleton<IBeatmapRepository, BeatmapRepository>()
                        .AddServiceKernel<IMatchmakingService, NodeService>()
                        .AddHostedService<MasterServerEventHandler>()
                );
    }
}