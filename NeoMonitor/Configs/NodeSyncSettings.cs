namespace NeoMonitor.Configs
{
    public sealed class NodeSyncSettings
    {
        public int ExceptionFilter { get; set; }

        public int ParallelDegree { get; set; } = 100;

        public int HostExecuteIntervalMilliseconds { get; set; } = 5000;
    }
}