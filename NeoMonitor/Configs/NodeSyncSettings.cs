namespace NeoMonitor.Configs
{
    public sealed class NodeSyncSettings
    {
        public int ExceptionFilter { get; set; }

        public int ParallelDegree { get; set; } = 100;
    }
}