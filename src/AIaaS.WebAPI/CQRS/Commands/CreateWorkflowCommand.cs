using AIaaS.WebAPI.Models;
using MediatR;

namespace AIaaS.WebAPI.CQRS.Commands
{
    public class CreateWorkflowCommand : IRequest<WorkflowDto>
    {
        public string WorkflowName { get; set; } = $"Workflow-created ({DateTime.Now})";
    }
}
