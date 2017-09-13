using System.IO;
using Microsoft.AspNetCore.Hosting;
using log4net;
using Carver.Config;
using Carver.Logging;

namespace Carver.API
{
    public static class ServiceRunner
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ServiceRunner));

        public static void Run()
        {
            Logger.InitLog4Net();
            Log.Info("Starting carver server.");

            string ip = Configuration.GetValue<string>("api_host", "http://localhost");
            int port = Configuration.GetValue<int>("api_port", 8086);

            var host = new WebHostBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseKestrel()
                .UseStartup<Startup>()
                .UseUrls($"{ip}:{port}")
                .Build();

            host.Run();
        }
    }
}