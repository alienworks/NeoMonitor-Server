using System;
using System.Collections.Generic;

namespace NeoMonitor.Rpc
{
    public class RpcRequestBody
    {
        public RpcRequestBody(string method)
        {
            Method = method ?? throw new ArgumentNullException("Property 'Method' cannot be null");
        }

        public string JsonRpc { get; set; }

        public string Method { get; set; }

        public IEnumerable<object> Params { get; set; }

        public long Id { get; set; }
    }
}