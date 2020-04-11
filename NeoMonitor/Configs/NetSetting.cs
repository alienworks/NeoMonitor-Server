using NeoMonitor.Abstractions.Constants;

namespace NeoMonitor.Configs
{
    public class NetSettings
    {
        public string Net { get; set; }
        public int[] MainNetPorts { get; set; }
        public int[] TestNetPorts { get; set; }

        public int[] GetPorts() => Net == NetConstants.MAIN_NET ? MainNetPorts : TestNetPorts;
    }
}