//using BeatTogether.Core.Messaging.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
//using BeatTogether.DedicatedServer.Kernel.Commands.CommandHandlers;
using BeatTogether.DedicatedServer.Kernel.Configuration;
using BeatTogether.DedicatedServer.Kernel.ENet;
//using BeatTogether.DedicatedServer.Kernel.Handshake;
using BeatTogether.DedicatedServer.Kernel.Managers;
using BeatTogether.DedicatedServer.Kernel.Managers.Abstractions;
using BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.MPChat;
//using BeatTogether.DedicatedServer.Messaging.Registries;
//using BeatTogether.DedicatedServer.Messaging.Registries.Unconnected;
using BeatTogether.Extensions;
/*using BeatTogether.LiteNetLib;
using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Configuration;
using BeatTogether.LiteNetLib.Dispatchers;
using BeatTogether.LiteNetLib.Extensions;
using BeatTogether.LiteNetLib.Sources;*/
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
                        //.AddLiteNetMessaging()
                        //.AddConfiguration<LiteNetConfiguration>("LiteNetLib")
                        .AddScoped<InstanceConfiguration>()
                        .AddDedicatedServerMessaging()
                        .AddScoped<DedicatedInstance>()
                        .AddExisting<IDedicatedInstance, DedicatedInstance>()
                        .AddExisting<ENetServer, DedicatedInstance>()
                        //.AddScoped<IHandshakeSessionRegistry, HandshakeSessionRegistry>()
                        .AddScoped<IPlayerRegistry, PlayerRegistry>()
                        //.AddScoped<UnconnectedMessageSource, UnconnectedSource>()
                        .AddScoped<PacketSource>()
                        //.AddScoped<UnconnectedDispatcher>()
                        //.AddExisting<UnconnectedMessageDispatcher, UnconnectedDispatcher>()
                        //.AddExisting<IUnconnectedDispatcher, UnconnectedDispatcher>()
                        .AddScoped<PacketDispatcher>()
                        .AddExisting<IPacketDispatcher, PacketDispatcher>()
                        //.AddExisting<ConnectedMessageDispatcher, PacketDispatcher>()
                        //.AddScoped<IHandshakeService, HandshakeService>()
                        .AddScoped<ILobbyManager, LobbyManager>()
                        .AddScoped<IGameplayManager, GameplayManager>()
                        //.AddCoreMessaging()
                        .AddSingleton<ITextCommandRepository, TextCommandRepository>()
                        //.AddSingleton<IMessageRegistry, HandshakeMessageRegistry>()
                        //.AddSingleton<IMessageRegistry, GameLiftMessageRegistry>()
                        //.AddAllHandshakeMessageHandlersFromAssembly(typeof(UnconnectedSource).Assembly)
                        .AddAllPacketHandlersFromAssembly(typeof(PacketSource).Assembly)
                        .AddAllCommandHandlersFromAssembly(typeof(MpcTextChatPacketHandler).Assembly)
                );
    }
}
