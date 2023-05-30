using AIaaS.WebAPI.Models;
using Microsoft.ML;

namespace AIaaS.WebAPI.Interfaces
{
    public interface IWorkflowOperator
    {
        string Name { get; set; }
        string Type { get; set; }
        Task Execute(WorkflowContext mlContext, Models.Dtos.WorkflowNodeDto root);
    }
}
