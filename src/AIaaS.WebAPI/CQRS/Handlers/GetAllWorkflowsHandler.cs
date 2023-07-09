using AIaaS.WebAPI.CQRS.Queries;
using AIaaS.WebAPI.Data;
using AIaaS.WebAPI.Models;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AIaaS.WebAPI.CQRS.Handlers
{
    public class GetAllWorkflowsHandler : IRequestHandler<GetAllWorkflowsQuery, IList<WorkflowItemDto>>
    {
        private readonly EfContext _dbContext;
        private readonly IMapper _mapper;

        public GetAllWorkflowsHandler(EfContext dbContext, IMapper mapper)
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
