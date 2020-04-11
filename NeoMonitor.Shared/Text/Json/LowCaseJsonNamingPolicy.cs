using System.Text.Json;

namespace NeoMonitor.Shared.Text.Json
{
    public sealed class LowCaseJsonNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name) => name.ToLower();
    }
}