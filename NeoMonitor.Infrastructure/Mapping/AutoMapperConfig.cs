using AutoMapper;
using NeoMonitor.Data.Models;
using NodeMonitor.ViewModels;

namespace NeoMonitor.Infrastructure.Mapping
{
    public class AutoMapperConfig
    {
        public static MapperConfiguration InitMap()
        {
            return new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Node, NodeViewModel>();
                cfg.CreateMap<NodeViewModel, Node>();
                cfg.CreateMap<NodeException, NodeException>();
            });
        }
    }
}