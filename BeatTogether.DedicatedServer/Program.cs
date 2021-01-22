using BeatTogether.Extensions;
using Microsoft.Extensions.Hosting;

namespace BeatTogether.DedicatedServer
{
    public class Program
    {
        public static void Main(string[] args) =>
            CreateHostBuilder(args).Build().Run();

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args).UseDedicatedServerKernel();
    }
}
