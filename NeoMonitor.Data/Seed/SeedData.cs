using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NeoMonitor.Data.Models;
using NeoState.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
			var mainNodes = ((JArray)JsonConvert.DeserializeObject(File.ReadAllText($@"seed-{net.ToLower()}.json"))).ToObject<List<NodeViewModel>>();
			foreach (var node in mainNodes)
			{
				var newNode = new Node
				{
					Id = 0,
					Url = node.Url,
					IP = node.IP,
					Type = Enum.Parse<NodeAddressType>(node.Type),
					Locale = node.Locale,
					Location = node.Location,
					Net = net
				};
				_ctx.Nodes.Add(newNode);
				_ctx.SaveChanges();
			}
		}
	}
}