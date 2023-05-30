using AIaaS.WebAPI.Models.Dtos;
using Ardalis.Result;

namespace AIaaS.WebAPI.Interfaces
{
    public interface IWorkflowService
    {
        Task<Result> Run(WorkflowGraphDto workflowGraphDto, Models.Workflow workflow);
    }
}
