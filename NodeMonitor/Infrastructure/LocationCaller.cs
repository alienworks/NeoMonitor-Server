using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NeoMonitor.Data;
using NeoMonitor.Data.Models;
using NeoState.Common;
using Newtonsoft.Json;

namespace NodeMonitor.Infrastructure
{
	public class LocationCaller
	{
		private Dictionary<string, Tuple<string, string, double, double>> IPs = new Dictionary<string, Tuple<string, string, double, double>>();

		private readonly NeoMonitorContext _ctx;

		public LocationCaller(NeoMonitorContext ctx)
		{
			_ctx = ctx;
		}

		public async Task UpdateAllNodeLocations()
		{
			var nodes = _ctx.Nodes.Where(node => !node.Latitude.HasValue || !node.Longitude.HasValue).ToList();
			foreach (var node in nodes)
			{
				await UpdateNode(node);
			}
		}

		public async Task UpdateNodeLocation(int nodeId)
		{
			var node = _ctx.Nodes.FirstOrDefault(n => n.Id == nodeId);

			if (node != null)
			{
				var result = await UpdateNode(node);
				if (result)
				{
					return;
				}
			}
		}

		private async Task<bool> UpdateNode(Node node)
		{
			try
			{
				if (node.Latitude == null || node.Longitude == null)
				{
					var response = await CheckIpCall(node.IP);
					if (response.IsSuccessStatusCode)
					{
						var responseText = await response.Content.ReadAsStringAsync();
						var responseOject = JsonConvert.DeserializeObject<LocationModel>(responseText);

						node.FlagUrl = responseOject.Flag;
						node.Location = responseOject.CountryName;
						node.Latitude = responseOject.Latitude;
						node.Longitude = responseOject.Longitude;

						return true;
					}
				}
			}
			catch (Exception e)
			{
				return false;
			}
			return false;
		}

		private static async Task<HttpResponseMessage> CheckIpCall(string ip)
		{
			HttpResponseMessage response;
			try
			{
				using (var http = new HttpClient())
				{
					var req = new HttpRequestMessage(HttpMethod.Get, $"http://api.ipstack.com/{ip}?access_key=86e45b940f615f26bba14dde0a002bc3");
					response = await http.SendAsync(req);
				}
			}
			catch (Exception e)
			{
				response = new HttpResponseMessage(HttpStatusCode.BadRequest);
			}

			return response;
		}
	}
}