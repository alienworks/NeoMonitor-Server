using System.Threading.Tasks;

namespace NodeMonitor.Web.Abstraction.Hubs
{
    public interface INodeClient
    {
        Task ShowServerMsg(string msg);

        Task ReceiveRawMemPoolInfosByIds(string json);

        Task UpdateRawMemPoolInfos(string json);
    }
}