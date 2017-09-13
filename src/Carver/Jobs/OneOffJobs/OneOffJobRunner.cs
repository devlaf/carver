using System;

namespace Carver.Jobs.OneOffJobs
{
    public static class OneOffJobRunner
    {
        public static void Run(string[] args)
        {
            if (args.Length != 3)
            {
                PrintUsage();
                return;
            }

            if (!GetJob(args[1], out IOneOffJob job))
                return;

            job.SetOptions(args[2]);
            job.Run();
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage: dotnet run -- job [jobname] [args]");
        }

        private static bool GetJob(string className, out IOneOffJob job)
        {
            try
            {
                Type classType = Type.GetType(className, true);
                job = (IOneOffJob)Activator.CreateInstance(classType);
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