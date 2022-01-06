using BeatTogether.DedicatedServer.Interface;
using BeatTogether.DedicatedServer.Node.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BeatTogether.DedicatedServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            var nodeservice = host.Services.GetRequiredService<IMatchmakingService>();
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args).UseDedicatedServerNode();
    }
}
