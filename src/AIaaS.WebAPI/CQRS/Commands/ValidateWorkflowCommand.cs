using AIaaS.WebAPI.Models;
using Ardalis.Result;
using MediatR;

namespace AIaaS.WebAPI.CQRS.Commands
{
    public class ValidateWorkflowCommand : IRequest<Result<WorkflowDto>>
    {
        public ValidateWorkflowCommand(WorkflowDto workflowDto)
        {
            WorkflowDto = workflowDto;
        }

        public WorkflowDto WorkflowDto { get; }
    }
}
