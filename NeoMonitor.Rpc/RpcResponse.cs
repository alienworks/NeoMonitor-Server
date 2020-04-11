namespace NeoMonitor.Rpc
{
    public struct RpcResponse<T>
    {
        public RpcResponse(bool success)
        {
            Success = success;
            ErrorMsg = string.Empty;
            Body = default;
        }

        public bool Success { get; }

        public string ErrorMsg { get; set; }

        public RpcResponseBody<T> Body { get; set; }
    }
}