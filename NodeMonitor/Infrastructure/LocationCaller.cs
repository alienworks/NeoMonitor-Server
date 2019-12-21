using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NeoMonitor.Data;
using NeoMonitor.Data.Models;
using NeoState.Common;
using NeoState.Common.Tools;

namespace NodeMonitor.Infrastructure
{
	public class LocationCaller
	{
		//private readonly Dictionary<string, (string, string, double, double)> IPs = new Dictionary<string, (string, string, double, double)>();

		private readonly NeoMonitorContext _ctx;

		public LocationCaller(NeoMonitorContext ctx)
		{
			_ctx = ctx;
		}

		public async Task UpdateAllNodeLocationsAsync()
		{
			var nodes = _ctx.Nodes.Where(node => node.Latitude == null || node.Longitude == null).ToList();
			foreach (var node in nodes)
			{
				await UpdateNodeAsync(node);
			}
			//var tasks = new Task[nodes.Count];
			//for (int i = 0; i < nodes.Count; i++)
			//{
			//	tasks[i] = UpdateNodeAsync(nodes[i]);
			//}
			//await Task.WhenAll(tasks);
		}

		public Task<bool> UpdateNodeAsync(int nodeId)
		{
			var node = _ctx.Nodes.FirstOrDefault(n => n.Id == nodeId);
			if (node is null)
			{
				return Task.FromResult(false);
			}
			return UpdateNodeAsync(node);
		}

		private async Task<bool> UpdateNodeAsync(Node node)
		{
			if (!node.Latitude.HasValue || !node.Longitude.HasValue)
			{
				using var rsp = await CheckIpCallAsync(node.IP);
				if (rsp.IsSuccessStatusCode)
				{
					string rspText = await rsp.Content.ReadAsStringAsync();
					var locModel = JsonTool.DeserializeObject<LocationModel>(rspText);
					if (locModel != null)
					{
						FillNodeLocationInfo(node, locModel);
						return true;
					}
				}
			}
			return false;
		}

		private static async Task<HttpResponseMessage> CheckIpCallAsync(string ip)
		{
			using var httpClient = new HttpClient();///TODO: should use IHttpClientFactory here?
			var req = new HttpRequestMessage(HttpMethod.Get, $"http://api.ipstack.com/{ip}?access_key=86e45b940f615f26bba14dde0a002bc3");///TODO: should add into config?
			HttpResponseMessage response = null;
			try
			{
				response = await httpClient.SendAsync(req);
			}
			catch (Exception)
			{
				response?.Dispose();
				response = new HttpResponseMessage(HttpStatusCode.BadRequest);
			}
			return response;
		}

		private static void FillNodeLocationInfo(Node node, LocationModel locModel)
		{
			node.FlagUrl = locModel.Flag;
			node.Location = locModel.CountryName;
			node.Latitude = locModel.Latitude;
			node.Longitude = locModel.Longitude;
		}
	}
}