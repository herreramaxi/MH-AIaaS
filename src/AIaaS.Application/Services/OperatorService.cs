using AIaaS.Application.Common.Models.CustomAttributes;
using AIaaS.Application.Common.Models.Dtos;
using AIaaS.Domain.Entities;
using AIaaS.Domain.Entities.enums;
using AIaaS.Domain.Interfaces;
using AIaaS.WebAPI.Interfaces;

namespace AIaaS.WebAPI.Services
{
    public interface IOperatorService
    {
        IList<OperatorDto> GetOperators();
        Task UpdateModel(Workflow workflow, MemoryStream modelData, CancellationToken cancellationToken);
        Task UpdateModelMetrics(Workflow workflow, MetricTypeEnum metricType, string modelSerialized, CancellationToken cancellationToken);
        Task<WorkflowDataView> AddWorkflowDataView(Workflow workflow, string nodeId, string nodeType, MemoryStream dataViewStream, CancellationToken cancellationToken);
    }
    public class OperatorService : IOperatorService
    {
        private readonly IRepository<Workflow> _workflowRepository;

        public OperatorService(IRepository<Workflow> workflowRepository)
        {
            _workflowRepository = workflowRepository;
        }
        public IList<OperatorDto> GetOperators()
        {
            var operators = new List<OperatorDto>();
            var workflowOperators = typeof(IWebApiMarker)
                .Assembly
                .GetTypes()
                .Where(x => typeof(IWorkflowOperator).IsAssignableFrom(x) && !x.IsAbstract && !x.IsInterface);

            foreach (var workflowOperator in workflowOperators)
            {
                var operatorAttribute = (OperatorAttribute?)workflowOperator.GetCustomAttributes(typeof(OperatorAttribute), false).FirstOrDefault();
                if (operatorAttribute is null) continue;

                var name = operatorAttribute.Name;
                var order = operatorAttribute.Order;
                var type = operatorAttribute.Type;

                var operatorDto = new OperatorDto()
                {
                    Name = name,
                    Type = type,
                    Order = order,
                    Data = new OperatorDataDto()
                    {
                        Name = name
                    }
                };

                operators.Add(operatorDto);

                var operatorParameters = (OperatorParameterAttribute[]?)workflowOperator.GetCustomAttributes(typeof(OperatorParameterAttribute), false);
                if (operatorParameters is null) continue;

                operatorDto.Data.Config = operatorParameters.Select(x => new OperatorConfigurationDto
                {
                    Name = x.Name,
                    Description = x.Description,
                    Type = x.Type,
                    Default = x.Default
                }).ToList();
            }

            return operators.OrderBy(x => x.Name).ToList();
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
    }
}
