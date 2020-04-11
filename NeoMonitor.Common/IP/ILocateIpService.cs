using System.Threading.Tasks;
using NeoMonitor.Common.IP.Models;

namespace NeoMonitor.Common.IP
{
    public interface ILocateIpService
    {
        Task<IpCheckModel> GetLocationAsync(string ip);
    }
}