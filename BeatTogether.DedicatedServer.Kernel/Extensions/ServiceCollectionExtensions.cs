using System.Linq;
using System.Reflection;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.CommandHandlers;
using Microsoft.Extensions.DependencyInjection;

namespace BeatTogether.Extensions
{
    public static class ServiceCollectionExtensions
    {
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

        public static IServiceCollection AddAllCommandHandlersFromAssembly(this IServiceCollection services, Assembly assembly)
        {
            var genericInterface = typeof(ICommandHandler<>);
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

/*        public static IServiceCollection AddAllHandshakeMessageHandlersFromAssembly(this IServiceCollection services, Assembly assembly)
        {
            var genericInterface = typeof(IHandshakeMessageHandler<>);
            var eventHandlerTypes = assembly
                .GetTypes()
                .Where(type => type.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == genericInterface));
            foreach (var eventHandlerType in eventHandlerTypes)
                if (!eventHandlerType.IsAbstract)
                    services.AddTransient(
                        genericInterface.MakeGenericType(eventHandlerType.BaseType!.GetGenericArguments()),
                        eventHandlerType);
            return services;
        }*/
    }
}
