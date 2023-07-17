using AIaaS.Application.Common.Models;
using AIaaS.Domain.Entities;
using AutoMapper;

namespace AIaaS.Application.Common.Mappings
{
    public class WorkflowRunHistoryProfile : Profile
    {
        public WorkflowRunHistoryProfile()
        {
            CreateMap<WorkflowRunHistory, WorkflowRunHistoryDto>()
                .ForMember(x => x.StatusHumanized, opt => opt.MapFrom(y => y.Status.ToString()))
                .ForMember(x => x.Milliseconds, opt => opt.MapFrom(y => y.Duration != null ? (int?)y.Duration.Value.Milliseconds : null))
                .ForMember(x => x.Seconds, opt => opt.MapFrom(y => y.Duration != null ? (int?)y.Duration.Value.Seconds : null))
                .ForMember(x => x.Minutes, opt => opt.MapFrom(y => y.Duration != null ? (int?)y.Duration.Value.Minutes : null));
        }
    }
}
