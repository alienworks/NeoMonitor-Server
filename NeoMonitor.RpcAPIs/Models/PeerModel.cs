using System.Collections.Generic;

namespace NeoMonitor.RpcAPIs.Models
{
    public sealed class PeerModel
    {
        public List<PeerChildModel> Unconnected { get; set; }

        public List<PeerChildModel> Bad { get; set; }

        public List<PeerChildModel> Connected { get; set; }
    }

    public sealed class PeerChildModel
    {
        public string Address { get; set; }

        public int Port { get; set; }
    }
}