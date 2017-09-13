using System;
using System.Runtime.CompilerServices;
using Carver.API;
using Carver.Jobs.OneOffJobs;
using Carver.Jobs.CronJobs;

[assembly: InternalsVisibleTo("CarverTests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace Carver
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                PrintUsage();
                return;
            }
            
            switch (args[0].ToLower())
            {
                case "job":
                    OneOffJobRunner.Run(args);
                    break;
                case "cronjob":
                    CronJobRunner.Run(args);
                    break;
                case "service":
                    ServiceRunner.Run();
                    break;
                case "help":
                default:
                    PrintUsage();
                    break;
            }
        }

        private static void PrintUsage()
        {
            Console.WriteLine("dotnet run -- [service | job | cronjob]");
        }
    }
}