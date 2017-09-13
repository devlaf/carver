namespace Carver.Jobs.OneOffJobs
{
    public interface IOneOffJob
    {
        string Usage();
        void SetOptions(string args);
        void Run();
    }
}