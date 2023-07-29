using AIaaS.Application.Common.Models;
using AIaaS.Application.Specifications.WorkflowRunHistories;
using AIaaS.Domain.Entities;
using AIaaS.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace AIaaS.Application.Features.WorkflowJobs.Queries.GetWorkflowJobs
{
    public class GetWorkflowJobDetailsHandler : IRequestHandler<GetWorkflowJobDetailsRequest, IList<WorkflowNodeRunHistoryDto>>
    {
        private readonly IReadRepository<WorkflowNodeRunHistory> _repository;
        private readonly IMapper _mapper;

        public GetWorkflowJobDetailsHandler(IReadRepository<WorkflowNodeRunHistory> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }
        public async Task<IList<WorkflowNodeRunHistoryDto>> Handle(GetWorkflowJobDetailsRequest request, CancellationToken cancellationToken)
        {
            var workflowRuns = await _repository.ListAsync(new GetWorkflowNodeRunHistoriesByRunId(request.WorkflowRunId));
            var dtos = _mapper.Map<IList<WorkflowNodeRunHistoryDto>>(workflowRuns);
            return dtos;
        }
    }
}
