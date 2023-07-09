using AIaaS.WebAPI.Models;
using Ardalis.Result;
using MediatR;

namespace AIaaS.WebAPI.CQRS.Commands
{
    public class RemoveWorkflowCommand : IRequest<Result>
    {
        public RemoveWorkflowCommand(int  workflowId)
        {
            WorkflowId = workflowId;
        }

        public int WorkflowId { get; }
    }
}
