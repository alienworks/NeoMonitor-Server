using System.Threading.Tasks;

namespace NodeMonitor.Web.Abstraction.Hubs
{
    public interface INodeCaller
    {
        Task ShowServerMsgAsync(string msg);

        Task SendRawMemPoolInfosByIdsAsync(string json);
    }
}