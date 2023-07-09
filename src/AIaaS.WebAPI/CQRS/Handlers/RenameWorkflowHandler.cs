using AIaaS.WebAPI.CQRS.Commands;
using AIaaS.WebAPI.Data;
using AIaaS.WebAPI.Models;
using Ardalis.Result;
using AutoMapper;
using MediatR;

namespace AIaaS.WebAPI.CQRS.Handlers
{
    public class RenameWorkflowHandler : IRequestHandler<RenameWorkflowCommand, Result<WorkflowDto>>
    {
        private readonly EfContext _dbContext;
        private readonly IMapper _mapper;

        public RenameWorkflowHandler(EfContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }
        public async Task<Result<WorkflowDto>> Handle(RenameWorkflowCommand request, CancellationToken cancellationToken)
        {
            var workflow = await _dbContext.Workflows.FindAsync(request.RenameParameter.Id);

            if (workflow is null) return Result.NotFound();

            workflow.Name = request.RenameParameter.Name;
            workflow.Description = request.RenameParameter.Description;

            _dbContext.Workflows.Update(workflow);
            await _dbContext.SaveChangesAsync();

            var mapped = _mapper.Map<Workflow, WorkflowDto>(workflow);

            return Result.Success(mapped);
        }
    }
}
