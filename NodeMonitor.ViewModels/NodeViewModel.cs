namespace NodeMonitor.ViewModels
{
	public class NodeViewModel
	{
		public int Id { get; set; }
		public string Url { get; set; }
		public string IP { get; set; }
		public string Type { get; set; }
		public string Version { get; set; }
		public int Height { get; set; }
		public long latency { get; set; }
		public int Peers { get; set; }
		public int MemPool { get; set; }
		public int ExceptionCount { get; set; }

		public string Locale { get; set; }
		public string Location { get; set; }
		public double? Longitude { get; set; }
		public double? Latitude { get; set; }
		public string FlagUrl { get; set; }

		public string Net { get; set; }

		public override int GetHashCode()
		{
			var hash = Url.GetHashCode();
			return hash;
		}
	}
}