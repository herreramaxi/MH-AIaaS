using AIaaS.Application.Common.Models;
using AIaaS.Application.Common.Models.Dtos;

namespace AIaaS.WebAPI.Interfaces
{
    public interface IWorkflowOperator
    {
        string Name { get; set; }
        string Type { get; set; }
        void Preprocessing(WorkflowContext context, WorkflowNodeDto root);
        Task Hydrate(WorkflowContext context, WorkflowNodeDto root);
        Task Run(WorkflowContext context, WorkflowNodeDto root, CancellationToken cancellationToken);
        bool Validate(WorkflowContext context, WorkflowNodeDto root);
        void PropagateDatasetColumns(WorkflowContext context, WorkflowNodeDto root);
        Task GenerateOuput(WorkflowContext context, WorkflowNodeDto root, CancellationToken cancellationToken);
    }
}
