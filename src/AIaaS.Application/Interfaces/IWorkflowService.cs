using AIaaS.Domain.Entities;
using AIaaS.Domain.Entities.enums;

namespace AIaaS.WebAPI.Interfaces
{
    public interface IWorkflowService
    {
        Task<Workflow?> GetWorkflowByIdWithModel(int workflowId, CancellationToken cancellationToken);
        Task<Workflow?> WorkflowByIdIncludeAll(int workflowId, CancellationToken cancellationToken);
        Task UpdateWorkflowData(Workflow workflow, string data, CancellationToken cancellationToken);
        Task UpdateModel(Workflow workflow, MemoryStream modelData, CancellationToken cancellationToken);
        Task UpdateModelMetrics(Workflow workflow, MetricTypeEnum metricType, string modelSerialized, CancellationToken cancellationToken);
        Task<WorkflowDataView> AddWorkflowDataView(Workflow workflow, string nodeId, string nodeType, MemoryStream dataViewStream, CancellationToken cancellationToken);
    }
}
