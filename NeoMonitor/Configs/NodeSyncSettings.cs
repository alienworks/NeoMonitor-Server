namespace NeoMonitor.Configs
{
    public sealed class NodeSyncSettings
    {
        public int ExceptionFilter { get; set; }

        public int ParallelDegree { get; set; } = 100;

        public int NodeInfoSyncIntervalMilliseconds { get; set; } = 5000;

        public int RawMemPoolBroadcastIntervalSeconds { get; set; } = 10;

        public int MatrixSyncIntervalMinutes { get; set; } = 5;
    }
}