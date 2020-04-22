using AutoMapper;
using NeoMonitor.Abstractions.Models;
using NeoMonitor.Abstractions.ViewModels;

namespace NeoMonitor.Profiles
{
    public sealed class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Node, NodeViewModel>()
                .ForMember(view => view.Type, opt => opt.MapFrom(n => n.Type.ToString()));
            CreateMap<NodeException, NodeExceptionViewModel>();

            CreateMap<NeoMatrixItemEntity, NeoMatrixItemViewModel>();
        }
    }
}