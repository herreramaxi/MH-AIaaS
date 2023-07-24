using AIaaS.Application.Common.Models;
using AIaaS.Application.Specifications.WorkflowRunHistories;
using AIaaS.Domain.Entities;
using AIaaS.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace AIaaS.Application.Features.WorkflowJobs.Queries.GetWorkflowJobs
{
    public class GetWorkflowJobsHandler : IRequestHandler<GetWorkflowJobsRequest, IList<WorkflowRunHistoryDto>>
    {
        private readonly IReadRepository<WorkflowRunHistory> _repository;
        private readonly IMapper _mapper;

        public GetWorkflowJobsHandler(IReadRepository<WorkflowRunHistory> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }
        public async Task<IList<WorkflowRunHistoryDto>> Handle(GetWorkflowJobsRequest request, CancellationToken cancellationToken)
        {
            var workflowRuns = await _repository.ListAsync(new GetAllWorkflowRunHistoryWithWorkflowNameSpec(request.WorkflowId));
            var dtos = _mapper.Map<IList<WorkflowRunHistoryDto>>(workflowRuns);
            return dtos;
        }
    }
}
