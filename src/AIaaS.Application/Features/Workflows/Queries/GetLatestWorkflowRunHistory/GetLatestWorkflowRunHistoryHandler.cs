using AIaaS.Application.Common.Models;
using AIaaS.Application.Features.Workflows.Queries.GetLatestWorkflowRunHistory;
using AIaaS.Application.Specifications.WorkflowRunHistories;
using AIaaS.Domain.Entities;
using AIaaS.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace AIaaS.Application.Features.Workflows.Queries
{
    public class GetLatestWorkflowRunHistoryHandler : IRequestHandler<GetLatestWorkflowRunHistoryQuery, WorkflowRunHistoryDto?>
    {
        private readonly IReadRepository<WorkflowRunHistory> _repository;
        private readonly IMapper _mapper;

        public GetLatestWorkflowRunHistoryHandler(IReadRepository<WorkflowRunHistory> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<WorkflowRunHistoryDto?> Handle(GetLatestWorkflowRunHistoryQuery request, CancellationToken cancellationToken)
        {
            var workflowRunHistory = await _repository.FirstOrDefaultAsync(new LatestWorkflowRunHistorySpec(request.WorkflowId), cancellationToken);
            var mapped = _mapper.Map<WorkflowRunHistoryDto>(workflowRunHistory);
            return mapped;
        }
    }
}
