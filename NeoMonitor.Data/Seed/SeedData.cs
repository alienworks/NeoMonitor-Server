using System;
using System.IO;
using System.Linq;
using NeoMonitor.Data.Models;
using NeoState.Common;
using Newtonsoft.Json;
using NodeMonitor.ViewModels;

namespace NeoMonitor.Data.Seed
{
	public class SeedData
	{
		private readonly NeoMonitorContext _ctx;

		public SeedData(NeoMonitorContext ctx)
		{
			_ctx = ctx;
		}

		public void Init()
		{
			SeedNodes();
		}

		private void SeedNodes()
		{
			if (!_ctx.Nodes.Any())
			{
				SeedNodesByNetType(NetConstants.MAIN_NET);
				SeedNodesByNetType(NetConstants.TEST_NET);
			}
		}

		private void SeedNodesByNetType(string net)
		{
			string seedjson = File.ReadAllText($@"seed-{net.ToLower()}.json");
			var mainNodes = JsonConvert.DeserializeObject<NodeViewModel[]>(seedjson);
			_ctx.Nodes.AddRange(mainNodes.Select(node => new Node()
			{
				Url = node.Url,
				IP = node.IP,
				Type = Enum.Parse<NodeAddressType>(node.Type),
				Locale = node.Locale,
				Location = node.Location,
				Net = net
			}));
			_ctx.SaveChanges();
		}
	}
}