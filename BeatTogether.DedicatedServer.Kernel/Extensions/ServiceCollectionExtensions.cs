using BeatTogether.DedicatedServer.Kernel;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Reflection;

namespace BeatTogether.Extensions
{
    public static class ServiceCollectionExtensions
    {
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
