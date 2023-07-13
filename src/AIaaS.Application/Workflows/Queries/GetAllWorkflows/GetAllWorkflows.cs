
using AIaaS.Application.Common.Models;
using AIaaS.Domain.Entities;
using AutoMapper;
using CleanArchitecture.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AIaaS.Application.Workflows.Queries.GetAllWorkflows
{
    public class GetAllWorkflowsQuery : IRequest<IList<WorkflowItemDto>>
    {
    }

    public class GetAllWorkflowsHandler : IRequestHandler<GetAllWorkflowsQuery, IList<WorkflowItemDto>>
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly IMapper _mapper;

        public GetAllWorkflowsHandler(IApplicationDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }
        public async Task<IList<WorkflowItemDto>> Handle(GetAllWorkflowsQuery request, CancellationToken cancellationToken)
        {
            var workflows = await _dbContext.Workflows.ToListAsync();
            var mapped = _mapper.Map<IList<Workflow>, IList<WorkflowItemDto>>(workflows);
            return mapped;
        }
    }
}
