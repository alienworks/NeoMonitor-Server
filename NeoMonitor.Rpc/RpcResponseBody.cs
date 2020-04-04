namespace NeoMonitor.Rpc
{
    public class RpcResponseBody<T>
    {
        public string JsonRpc { get; set; }

        public long Id { get; set; }

        public T Result { get; set; }
    }
}