using AutoMapper;
using NeoMonitor.App.ViewModels;
using NeoMonitor.Basics.Models;

namespace NeoMonitor.App.Profiles
{
    public sealed class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Node, NodeViewModel>()
                .ForMember(view => view.Type, opt => opt.MapFrom(n => n.Type.ToString()));
            CreateMap<NodeException, NodeExceptionViewModel>();
        }
    }
}