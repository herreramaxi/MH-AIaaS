using AIaaS.Application.Common.Models;
using Ardalis.Result;
using MediatR;

namespace AIaaS.Application.Features.Workflows.Commands.ValidateAndSaveWorkflow
{
    public class ValidateAndSaveCommand : IRequest<Result<WorkflowDto>>
    {
        public ValidateAndSaveCommand(WorkflowDto workflowDto)
        {
            WorkflowDto = workflowDto;
        }

        public WorkflowDto WorkflowDto { get; }
    }
}
