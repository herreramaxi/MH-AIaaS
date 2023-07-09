using AIaaS.WebAPI.Models;
using Ardalis.Result;

namespace AIaaS.WebAPI.Interfaces
{
    public interface IWorkflowService
    {
        Task<Result<WorkflowDto>> Run(WorkflowDto workflowDto);
        Task<Result<WorkflowDto>> Validate(WorkflowDto workflowDto);
    }
}
