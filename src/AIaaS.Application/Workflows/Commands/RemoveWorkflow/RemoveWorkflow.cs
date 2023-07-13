
using Ardalis.Result;
using CleanArchitecture.Application.Common.Interfaces;
using MediatR;

namespace AIaaS.Application.Workflows.Commands.RemoveWorkflow
{
    public class RemoveWorkflowCommand : IRequest<Result>
    {
        public RemoveWorkflowCommand(int workflowId)
        {
            WorkflowId = workflowId;
        }

        public int WorkflowId { get; }
    }

    public class RemoveWorkflowHandler : IRequestHandler<RemoveWorkflowCommand, Result>
    {
        private readonly IApplicationDbContext _dbContext;

        public RemoveWorkflowHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<Result> Handle(RemoveWorkflowCommand request, CancellationToken cancellationToken)
        {
            var workflow = await _dbContext.Workflows.FindAsync(request.WorkflowId);
            if (workflow is null)
                return Result.NotFound();

            _dbContext.Workflows.Remove(workflow);
            await _dbContext.SaveChangesAsync();

            return Result.Success();
        }
    }
}
