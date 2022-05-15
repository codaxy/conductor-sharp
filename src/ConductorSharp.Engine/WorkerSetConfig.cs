namespace ConductorSharp.Engine
{

    public class WorkerSetConfig
    {
        public string Domain { get; set; }
        public int LongPollInterval { get; set; }
        public int MaxConcurrentWorkers { get; set; }
        public int SleepInterval { get; set; }
    }
}