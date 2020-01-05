using AutoMapper;
using NeoMonitor.Data.Models;
using NodeMonitor.ViewModels;

namespace NeoMonitor.Infrastructure.Mapping
{
    public class AutoMapperConfig
    {
        public static void InitMap(IMapperConfigurationExpression confExp)
        {
            confExp.CreateMap<Node, NodeViewModel>();
            confExp.CreateMap<NodeViewModel, Node>();
            confExp.CreateMap<NodeException, NodeException>();
            confExp.CreateMap<NodeException, NodeExceptionViewModel>();
        }
    }
}