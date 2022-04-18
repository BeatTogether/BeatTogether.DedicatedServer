using BeatTogether.DedicatedServer.Node.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinFormsLibrary;

namespace BeatTogether.DedicatedServer
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] args) {

            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var host = CreateHostBuilder(args).Build();

            var services = host.Services;
            var mainForm = services.GetService<DedicatedServerViews>();

            Task mytask = Task.Run(() =>
            {
                //mainForm.ShowDialog();
                Application.Run(mainForm);
            });


            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args).UseDedicatedServerNode();
    }
}
