using AIaaS.Application.Common.Models;
using AIaaS.Domain.Entities;
using AutoMapper;

namespace AIaaS.Application.Common.Mappings
{
    public class WorkflowNodeRunHistoryProfile : Profile
    {
        public WorkflowNodeRunHistoryProfile()
        {
            CreateMap<WorkflowNodeRunHistory, WorkflowNodeRunHistoryDto>()
                .ForMember(x => x.StatusHumanized, opt => opt.MapFrom(y => y.Status.ToString()));
        }
    }
}
