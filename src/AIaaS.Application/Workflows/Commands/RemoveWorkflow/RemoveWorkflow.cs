
using AIaaS.Application.Specifications;
using AIaaS.Domain.Entities;
using AIaaS.Domain.Interfaces;
using Ardalis.Result;
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
        private readonly IRepository<Workflow> _workflowRepository;

        public RemoveWorkflowHandler(IRepository<Workflow> workflowRepository)
        {
            _workflowRepository = workflowRepository;
        }
        public async Task<Result> Handle(RemoveWorkflowCommand request, CancellationToken cancellationToken)
        {
            var workflow = await _workflowRepository.FirstOrDefaultAsync(new WorkflowByIdSpec(request.WorkflowId), cancellationToken);
            if (workflow is null)
                return Result.NotFound();

            await _workflowRepository.DeleteAsync(workflow,cancellationToken);

            return Result.Success();
        }
    }
}
