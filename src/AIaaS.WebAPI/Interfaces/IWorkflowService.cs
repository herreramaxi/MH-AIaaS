using AIaaS.WebAPI.Models;
using AIaaS.WebAPI.Models.Dtos;
using Ardalis.Result;

namespace AIaaS.WebAPI.Interfaces
{
    public interface IWorkflowService
    {
        Task<Result<WorkflowGraphDto>> Run(WorkflowGraphDto workflowGraphDto, Workflow workflow);
        Task<Result<WorkflowGraphDto>> Validate(WorkflowGraphDto workflowGraphDto, Workflow workflow);
    }
}
