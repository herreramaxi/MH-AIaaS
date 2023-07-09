using AIaaS.WebAPI.CQRS.Commands;
using AIaaS.WebAPI.Data;
using AIaaS.WebAPI.Models;
using Ardalis.Result;
using AutoMapper;
using MediatR;

namespace AIaaS.WebAPI.CQRS.Handlers
{
    public class RemoveWorkflowHandler : IRequestHandler<RemoveWorkflowCommand, Result>
    {
        private readonly EfContext _dbContext;

        public RemoveWorkflowHandler(EfContext dbContext)
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
