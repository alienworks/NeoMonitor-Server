using System.Collections.Generic;
using System.Threading.Tasks;
using NeoMonitor.RpcAPIs.Models;

namespace NeoMonitor.RpcAPIs
{
    public interface INeoJsonRpcService
    {
        Task<int?> GetBlockCountAsync(string url, long id = 1);

        Task<List<string>> GetRawMemPoolAsync(string url, long id = 1);

        Task<PeerModel> GetPeersAsync(string url, long id = 1);

        Task<VersionModel> GetVersionAsync(string url, long id = 1);
    }
}