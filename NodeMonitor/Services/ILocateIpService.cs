using System.Threading.Tasks;
using NeoState.Common.Location;

namespace NodeMonitor.Services
{
    public interface ILocateIpService
    {
        Task<IpCheckModel> GetLocationAsync(string ip);
    }
}