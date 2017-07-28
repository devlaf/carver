using System;
using System.IO;
using System.Runtime.CompilerServices;
using Carver.API;
using Microsoft.AspNetCore.Hosting;

[assembly: InternalsVisibleTo("CarverTests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace Carver
{
    class Program
    {
        static void Main(string[] args)
        {
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