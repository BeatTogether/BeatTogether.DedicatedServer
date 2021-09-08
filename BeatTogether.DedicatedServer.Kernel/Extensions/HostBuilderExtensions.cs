using Autobus;
using BeatTogether.DedicatedServer.Interface;
using BeatTogether.DedicatedServer.Kernel;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Configuration;
using BeatTogether.DedicatedServer.Kernel.Factories;
using BeatTogether.DedicatedServer.Kernel.Managers;
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
                        .AddSingleton<IPacketDispatcher, PacketDispatcher>()
                        .AddSingleton<IPortAllocator, PortAllocator>()
                        .AddSingleton<IMatchmakingServerRegistry, MatchmakingServerRegistry>()
                        .AddSingleton<IMatchmakingServerFactory, MatchmakingServerFactory>()
                        .AddServiceKernel<IMatchmakingService, MatchmakingService>()
                        .AddHostedService<MasterServerEventHandler>()
                        .AddAsyncLocal<IMatchmakingServer, MatchmakingServer>()
                        .AddAsyncLocal<IPlayerRegistry, PlayerRegistry>()
                        .AddAsyncLocal<IPacketSource, PacketSource>()
                        .AddAsyncLocal<IPermissionsManager, PermissionsManager>()
                        .AddAsyncLocal<ILobbyManager, LobbyManager>()
                        .AddAsyncLocal<IEntitlementManager, EntitlementManager>()
                        .AddAllPacketHandlersFromAssembly(typeof(PacketSource).Assembly)
                );
    }
}