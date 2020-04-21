using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using NeoMonitor.Abstractions.Constants;
using NeoMonitor.Abstractions.Models;
using NeoMonitor.Abstractions.Services;

namespace NeoMonitor.Services.Seeds
{
    internal sealed class NodeSeedsLoader : INodeSeedsLoader
    {
        private const string SeedJsonFileNameFormat = "seed-{0}.json";

        public List<Node> Load()
        {
            var result = LoadFromJsonFile(NetConstants.MAIN_NET, NetConstants.TEST_NET);
            return result;
        }

        private List<Node> LoadFromJsonFile(params string[] nets)
        {
            var result = new List<Node>(64);
            foreach (string net in nets)
            {
                string file = string.Format(SeedJsonFileNameFormat, net.ToLower());
                var bytes = File.ReadAllBytes(file);
                var temp = JsonSerializer.Deserialize<List<Node>>(bytes, new JsonSerializerOptions() { AllowTrailingCommas = true, PropertyNameCaseInsensitive = true });
                temp.ForEach(n => n.Net = net);
                result.AddRange(temp);
            }
            return result;
        }
    }
}