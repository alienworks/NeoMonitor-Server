using System.Collections.Generic;

namespace NodeMonitor.ViewModels
{
	public sealed class NodeViewModelEqualityComparer : IEqualityComparer<NodeViewModel>
	{
		public static NodeViewModelEqualityComparer Instance { get; } = new NodeViewModelEqualityComparer();

		public bool Equals(NodeViewModel x, NodeViewModel y)
		{
			return x.Url == y.Url;
		}

		public int GetHashCode(NodeViewModel obj)
		{
			return obj.Url.GetHashCode();
		}
	}
}