using AIaaS.WebAPI.Models;
using AIaaS.WebAPI.Models.Dtos;
using Ardalis.Result;
using MediatR;

namespace AIaaS.WebAPI.CQRS.Commands
{
    public class RenameWorkflowCommand : IRequest<Result<WorkflowDto>>
    {
        public RenameWorkflowCommand(WorkflowRenameParameter renameParameter)
        {
            RenameParameter = renameParameter;
        }

        public WorkflowRenameParameter RenameParameter { get; }
    }
}
