using AutoMapper;
using NeoMonitor.Data.Models;
using NodeMonitor.ViewModels;

namespace NeoMonitor.Infrastructure.Mapping
{
	internal class NodeConfig
	{
		public static void InitMap(IMapperConfigurationExpression cfg)
		{
			cfg.CreateMap<Node, NodeViewModel>();

			cfg.CreateMap<NodeViewModel, Node>();

			cfg.CreateMap<NodeException, NodeException>();
		}
	}
}