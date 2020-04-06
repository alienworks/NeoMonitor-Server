namespace NeoMonitor.Analysis.Models
{
    public sealed class IpVisitAnaData
    {
        public long Id { get; set; }

        public string Ip { get; set; }

        public int Times { get; set; }

        public int Day { get; set; }

        public int Hour { get; set; }
    }
}