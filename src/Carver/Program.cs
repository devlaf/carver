using System;
using System.IO;
using Carver.API;
using Carver.DataStore;
using Carver.Users;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace Carver
{
    class Program
    {
        static void Main(string[] args)
        {
            string ip = Configuration.GetValue<string>("host", "http://localhost");
            int port = Configuration.GetValue<int>("port", 8086);

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