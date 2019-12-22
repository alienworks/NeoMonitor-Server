using System.Collections.Generic;
using Newtonsoft.Json;

namespace NeoState.Common.RPC
{
	public class RPCPeersResponse
	{
		[JsonProperty(PropertyName = "unconnected")]
		public List<RPCPeer> Unconnected { get; set; }

		[JsonProperty(PropertyName = "bad")]
		public List<RPCPeer> Bad { get; set; }

		[JsonProperty(PropertyName = "connected")]
		public List<RPCPeer> Connected { get; set; }
	}
}