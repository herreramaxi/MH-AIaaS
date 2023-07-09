using AIaaS.WebAPI.CQRS.Queries;
using AIaaS.WebAPI.Data;
using AIaaS.WebAPI.Models;
using Ardalis.Result;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AIaaS.WebAPI.CQRS.Handlers
{
    public class GetWorkflowByIdHandler : IRequestHandler<GetWorkflowByIdQuery, Result<WorkflowDto>>
    {
        private readonly EfContext _dbContext;
        private readonly IMapper _mapper;

        public GetWorkflowByIdHandler(EfContext dbContext, IMapper mapper)
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
