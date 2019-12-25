using System.Threading.Tasks;
using NeoState.Common;
using NodeMonitor.Services;

namespace NodeMonitor.Infrastructure
{
	public sealed class LocationCaller
	{
		//private readonly Dictionary<string, (string, string, double, double)> IPs = new Dictionary<string, (string, string, double, double)>();

		private readonly ILocateIpService _ipLocationService;

		public LocationCaller(ILocateIpService ipLocationService)
		{
			_ipLocationService = ipLocationService;
		}

		public Task<LocationModel> CheckIpCallAsync(string ip)
		{
			return _ipLocationService.GetLocationAsync(ip);
		}
	}
}