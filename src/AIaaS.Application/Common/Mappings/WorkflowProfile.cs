using AIaaS.Application.Common.Models;
using AIaaS.Domain.Entities;
using AutoMapper;

namespace AIaaS.WebAPI.Mappings
{
    public class WorkflowProfile : Profile
    {
        public WorkflowProfile()
        {
            CreateMap<Workflow, WorkflowDto>()
                .ForMember(dest => dest.IsPublished, opt => opt.MapFrom(src => src.MLModel != null && src.MLModel.Endpoint != null))
                .ForMember(dest => dest.IsModelGenerated, opt => opt.MapFrom(src => src.MLModel != null))
                .ForMember(dest => dest.Root, opt => opt.MapFrom(src => src.Data));

            CreateMap<Workflow, WorkflowItemDto>();
        }
    }
}
