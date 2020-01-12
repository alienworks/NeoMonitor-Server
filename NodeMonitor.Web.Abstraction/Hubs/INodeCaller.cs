using System.Threading.Tasks;

namespace NodeMonitor.Web.Abstraction.Hubs
{
    public interface INodeCaller
    {
        Task SendAsync();

        Task ShowMsgAsync(string msg);

        Task SendRawMemPoolInfosByIdsAsync(string json);
    }
}