using AIaaS.Application.Common.Models;
using AIaaS.Domain.Entities;
using Ardalis.Result;

namespace AIaaS.WebAPI.Interfaces
{
    public interface IWorkflowService
    {
        Task<Workflow?> GetWorkflowByIdWithModel(int workflowId, CancellationToken cancellationToken);
        Task<Workflow?> WorkflowByIdIncludeAll(int workflowId, CancellationToken cancellationToken);
        Task UpdateWorkflowData(Workflow workflow, string data, CancellationToken cancellationToken);       
        Task<Result<WorkflowDto>> Run(WorkflowDto workflowDto, WorkflowContext context, CancellationToken cancellationToken);
    }
}
