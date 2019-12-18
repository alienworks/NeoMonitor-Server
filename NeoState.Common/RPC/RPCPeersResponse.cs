using System.Collections.Generic;
using Newtonsoft.Json;

namespace NeoState.Common.RPC
{
	public class RPCPeersResponse
	{
		[JsonProperty(PropertyName = "unconnected")]
		public IEnumerable<RPCPeer> Unconnected { get; set; }

		[JsonProperty(PropertyName = "bad")]
		public IEnumerable<RPCPeer> Bad { get; set; }

		[JsonProperty(PropertyName = "connected")]
		public IEnumerable<RPCPeer> Connected { get; set; }
	}
}