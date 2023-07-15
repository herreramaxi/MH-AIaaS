using AIaaS.Application.Specifications;
using AIaaS.Domain.Entities;
using AIaaS.Domain.Entities.enums;
using AIaaS.Domain.Interfaces;
using AIaaS.WebAPI.Interfaces;

namespace AIaaS.WebAPI.Services
{
    public class WorkflowService : IWorkflowService
    {
        private readonly IRepository<Workflow> _workflowRepository;
        private readonly IReadRepository<Workflow> _readRepositoryWorkflow;

        public WorkflowService(IRepository<Workflow> workflowRepository, IReadRepository<Workflow> readRepositoryWorkflow)
        {
            _workflowRepository = workflowRepository;
            _readRepositoryWorkflow = readRepositoryWorkflow;
        }

        public async Task<Workflow?> GetWorkflowByIdWithModel(int workflowId, CancellationToken cancellationToken)
        {
            var spec = new WorkflowByIdWithModelSpec(workflowId);
            var workflow = await _readRepositoryWorkflow.FirstOrDefaultAsync(spec, cancellationToken);

            return workflow;
        }

        public async Task<Workflow?> WorkflowByIdIncludeAll(int workflowId, CancellationToken cancellationToken)
        {            
            var spec = new WorkflowByIdIncludeAllSpec(workflowId);
            var workflow = await _readRepositoryWorkflow.FirstOrDefaultAsync(spec, cancellationToken);

            return workflow;
        }

        public async Task UpdateModel(Workflow workflow, MemoryStream modelData, CancellationToken cancellationToken)
        {
            workflow.AddOrUpdateMLModelData(modelData);
            await _workflowRepository.UpdateAsync(workflow, cancellationToken);
        }

        public async Task UpdateModelMetrics(Workflow workflow, MetricTypeEnum metricType, string modelSerialized, CancellationToken cancellationToken)
        {
            workflow.MLModel?.UpdateModelMetrics(metricType, modelSerialized);
            await _workflowRepository.UpdateAsync(workflow, cancellationToken);
        }

        public async Task<WorkflowDataView> AddWorkflowDataView(Workflow workflow, string nodeId, string nodeType, MemoryStream dataViewStream, CancellationToken cancellationToken)
        {
            var dataView = workflow.AddOrUpdateDataView(nodeId, nodeType, dataViewStream);
            await _workflowRepository.UpdateAsync(workflow, cancellationToken);

            return dataView;
        }

        public async Task UpdateWorkflowData(Workflow workflow, string data, CancellationToken cancellationToken)
        {
            workflow.UpdateData(data);
            await _workflowRepository.UpdateAsync(workflow, cancellationToken);
        }       
    }
}
