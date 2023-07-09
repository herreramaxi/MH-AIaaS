using AIaaS.WebAPI.Models;
using Ardalis.Result;
using MediatR;

namespace AIaaS.WebAPI.CQRS.Commands
{
    public class SaveWorkflowCommand : IRequest<Result<WorkflowDto>>
    {
        public WorkflowSaveDto WorkflowSaveDto { get; }
        public SaveWorkflowCommand(WorkflowSaveDto workflowSaveDto)
        {
            WorkflowSaveDto = workflowSaveDto;
        }
    }
}
