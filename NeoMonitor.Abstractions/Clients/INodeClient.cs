using System.Threading.Tasks;

namespace NeoMonitor.Abstractions.Clients
{
    public interface INodeClient
    {
        Task ShowServerMsg(string msg);

        Task ReceiveRawMemPoolInfosByIds(string json);

        Task UpdateRawMemPoolInfos(string json);
    }
}