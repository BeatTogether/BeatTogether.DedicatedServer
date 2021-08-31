using Autobus;
using BeatTogether.DedicatedServer.Interface;
using BeatTogether.DedicatedServer.Kernel;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Configuration;
using BeatTogether.DedicatedServer.Kernel.Factories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Security.Cryptography;

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
                        .AddCoreSecurity()
                        .AddDedicatedServerMessaging()
                        .AddAutoMapper(configuration =>
                        {
                            configuration.CreateMap<DedicatedServer.Interface.Models.GameplayServerConfiguration,
                                                    DedicatedServer.Kernel.Models.GameplayServerConfiguration>();
                        })
                        .AddConfiguration<ServerConfiguration>("Server")
                        .AddTransient<RNGCryptoServiceProvider>()
                        .AddTransient(serviceProvider =>
                            new AesCryptoServiceProvider()
                            {
                                Mode = CipherMode.CBC,
                                Padding = PaddingMode.None
                            }
                        )
                        .AddSingleton<IEncryptedPacketReader, EncryptedPacketReader>()
                        .AddSingleton<IEncryptedPacketWriter, EncryptedPacketWriter>()
                        .AddSingleton<PacketEncryptionLayer>()
                        .AddAsyncLocal<IPacketSource>()
                        .AddSingleton<IPacketDispatcher, PacketDispatcher>()
                        .AddSingleton<IPortAllocator, PortAllocator>()
                        .AddAsyncLocal<IPlayerRegistry>()
                        .AddSingleton<IMatchmakingServerRegistry, MatchmakingServerRegistry>()
                        .AddSingleton<IMatchmakingServerFactory, MatchmakingServerFactory>()
                        .AddServiceKernel<IMatchmakingService, MatchmakingService>()
                        .AddHostedService<MasterServerEventHandler>()
                        .AddAsyncLocal<IServerContext>()
                        .AddAllPacketHandlersFromAssembly(typeof(PacketSource).Assembly)
                );
    }
}
