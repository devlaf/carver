namespace Carver.Jobs.CronJobs
{
    public interface ICronJob
    {
        string CronFormattedSchedule();
        void Run();
    }
}