using System;

namespace Carver.Jobs.CronJobs
{
    public class CronJobRunner
    {
        public static void Run(string[] args)
        {
            if (args.Length != 3)
            {
                PrintUsage();
                return;
            }

            if (!GetJob(args[1], out ICronJob job))
                return;
            
            switch (args[2].ToLower())
            {
                case "schedule":
                    Console.WriteLine(job.CronFormattedSchedule());
                    break;
                case "run":
                    job.Run();
                    break;
                case "help":
                default:
                    PrintUsage();
                    break;
            }
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage: dotnet run -- cronjob [jobname] [schedule | run]");
        }

        private static bool GetJob(string className, out ICronJob job)
        {
            try
            {
                Type classType = Type.GetType(className, true);
                job = (ICronJob)Activator.CreateInstance(classType);
                return true;
            }
            catch
            {
                Console.WriteLine("Invalid job name.");
                PrintUsage();
                job = null;
                return false;
            }

        }
    }
}