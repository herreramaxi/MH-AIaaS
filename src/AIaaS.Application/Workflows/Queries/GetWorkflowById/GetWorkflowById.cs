
using AIaaS.Application.Common.Models;
using Ardalis.Result;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using CleanArchitecture.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AIaaS.Application.Workflows.Queries.GetWorkflowById
{
    public class GetWorkflowByIdQuery : IRequest<Result<WorkflowDto>>
    {
        public int WorkflowId { get; }
        public GetWorkflowByIdQuery(int workflowId)
        {
            WorkflowId = workflowId;
        }
    }

    public class GetWorkflowByIdHandler : IRequestHandler<GetWorkflowByIdQuery, Result<WorkflowDto>>
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly IMapper _mapper;

        public GetWorkflowByIdHandler(IApplicationDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }
        public async Task<Result<WorkflowDto>> Handle(GetWorkflowByIdQuery request, CancellationToken cancellationToken)
        {
            var workflowDto = await _dbContext.Workflows
                .Where(w => w.Id == request.WorkflowId)
                .ProjectTo<WorkflowDto>(_mapper.ConfigurationProvider)
                .SingleOrDefaultAsync();

            return workflowDto is not null ?
                Result.Success(workflowDto) :
                Result.NotFound();
        }
    }
}
