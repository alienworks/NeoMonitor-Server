using System;
using Newtonsoft.Json;

namespace NeoState.Common.RPC
{
	public class RPCRequestBody : RPCBaseBody
	{
		[JsonProperty(PropertyName = "method")]
		public string Method { get; set; }

		[JsonProperty(PropertyName = "params")]
		public string[] Params { get; set; }

		public RPCRequestBody() : this("getblockcount")
		{
		}

		public RPCRequestBody(string method)
		{
			Method = method;
			Params = Array.Empty<string>();
		}
	}
}