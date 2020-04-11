namespace NeoMonitor.Rpc.APIs.Models
{
    public sealed class VersionModel
    {
        public int Port { get; set; }

        public long Nonce { get; set; }

        public string UserAgent { get; set; }
    }
}