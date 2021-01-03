using System;
using System.Reactive.Linq;
using BeatTogether.Core.Hosting.Extensions;
using BeatTogether.Core.Messaging.Bootstrap;
using BeatTogether.Core.Messaging.Configuration;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Abstractions.Providers;
using BeatTogether.DedicatedServer.Kernel.Configuration;
using BeatTogether.DedicatedServer.Kernel.Implementations;
using BeatTogether.DedicatedServer.Kernel.Implementations.Factories;
using BeatTogether.DedicatedServer.Kernel.Implementations.MessageHandlers;
using BeatTogether.DedicatedServer.Messaging.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Obvs;
using Obvs.Configuration;
using Obvs.RabbitMQ.Configuration;
using Obvs.Serialization.Json.Configuration;
using Serilog;

namespace BeatTogether.DedicatedServer.Kernel.Bootstrap
{
    public static class DedicatedServerKernelStartup
    {
        public static void ConfigureServices(HostBuilderContext hostBuilderContext, IServiceCollection services)
        {
            CoreMessagingBootstrapper.ConfigureServices(hostBuilderContext, services);

            services.AddConfiguration<DedicatedServerConfiguration>(hostBuilderContext.Configuration, "DedicatedServer");
            services.AddConfiguration<RelayServerConfiguration>(hostBuilderContext.Configuration, "DedicatedServer:RelayServers");

            services.AddSingleton(serviceProvider =>
            {
                var rabbitMQConfiguration = serviceProvider.GetRequiredService<RabbitMQConfiguration>();
                Log.Information($"Building service bus (EndPoint='{rabbitMQConfiguration.EndPoint}').");
                var serviceBus = ServiceBus.Configure()
                    .WithRabbitMQEndpoints<IDedicatedServerMessage>()
                        .Named("DedicatedServer")
                        .ConnectToBroker(rabbitMQConfiguration.EndPoint)
                        .SerializedAsJson()
                        .AsServer()
                    .Create();
                serviceBus.Exceptions.Subscribe(e => Log.Error(e, $"Handling service bus exception."));
                return serviceBus;
            });

            services.AddSingleton<IDedicatedServerPortAllocator, DedicatedServerPortAllocator>();
            services.AddSingleton<IRelayServerFactory, RelayServerFactory>();
            services.AddScoped<IDedicatedServerService, DedicatedServerService>();
            services.AddHostedService<DedicatedServerMessageHandler>();
        }
    }
}
