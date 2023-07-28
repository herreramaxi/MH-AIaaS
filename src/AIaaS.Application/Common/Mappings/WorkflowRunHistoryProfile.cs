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
                .ForMember(x => x.StatusHumanized, opt => opt.MapFrom(y => y.Status.ToString()));
        }
    }
}
