using System.Threading.Tasks;
using NeoState.Common;

namespace NodeMonitor.Services
{
	public interface ILocateIpService
	{
		Task<LocationModel> GetLocationAsync(string ip);
	}
}