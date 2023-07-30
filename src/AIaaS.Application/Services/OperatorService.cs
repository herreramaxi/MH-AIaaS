using AIaaS.Application.Common.Models.CustomAttributes;
using AIaaS.Application.Common.Models.Dtos;
using AIaaS.Domain.Entities;
using AIaaS.Domain.Entities.enums;
using AIaaS.Domain.Interfaces;
using AIaaS.WebAPI.ExtensionMethods;
using AIaaS.WebAPI.Interfaces;
using Ardalis.Result;
using Microsoft.Extensions.Logging;

namespace AIaaS.WebAPI.Services
{
    public interface IOperatorService
    {
        IList<OperatorDto> GetOperators();
        Task UpdateModel(Workflow workflow, MemoryStream modelData, CancellationToken cancellationToken);
        Task UpdateModelMetrics(Workflow workflow, MetricTypeEnum metricType, string modelSerialized, CancellationToken cancellationToken);
        Task<Result<WorkflowDataView>> GenerateWorkflowDataView(Workflow workflow, Guid? nodeGuid, string nodeType, MemoryStream dataViewStream, CancellationToken cancellationToken);
    }
    public class OperatorService : IOperatorService
    {
        private readonly IRepository<Workflow> _workflowRepository;
        private readonly IRepository<WorkflowDataView> _workflowDataViewRepository;
        private readonly IS3Service _s3Service;
        private readonly ILogger<OperatorService> _logger;

        public OperatorService(
            IRepository<Workflow> workflowRepository,
            IRepository<WorkflowDataView> workflowDataViewRepository,
            IS3Service s3Service,
            ILogger<OperatorService> logger)
        {
            _workflowRepository = workflowRepository;
            _workflowDataViewRepository = workflowDataViewRepository;
            _s3Service = s3Service;
            _logger = logger;
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

        public async Task<Result<WorkflowDataView>> GenerateWorkflowDataView(Workflow workflow, Guid? nodeGuid, string nodeType,  MemoryStream dataViewStream, CancellationToken cancellationToken)
        {
            if (nodeGuid is null) return Result.Error("NodeGuid not provided");

            try
            {
                var dataView = workflow.WorkflowDataViews.FirstOrDefault(x => x.NodeGuid  == nodeGuid);

                if (dataView is null)
                {
                    dataView = new WorkflowDataView
                    {
                        WorkflowId = workflow.Id,
                        NodeGuid = nodeGuid.Value,
                        NodeType = nodeType,
                        Size = dataViewStream.Length,
                        S3Key = $"workflowDataView_{workflow.Id}_{nodeGuid}_{nodeType}.idv".GenerateS3Key()
                    };

                    await _workflowDataViewRepository.AddAsync(dataView, cancellationToken);
                }
                else
                {
                    dataView.S3Key = !string.IsNullOrEmpty(dataView.S3Key) ? dataView.S3Key : $"workflowDataView_{workflow.Id}_{nodeGuid}_{nodeType}.idv".GenerateS3Key();
                    dataView.Size = dataViewStream.Length;
                    await _workflowDataViewRepository.UpdateAsync(dataView, cancellationToken);
                }

                var result = await _s3Service.UploadFileAsync(dataViewStream, dataView.S3Key);

                return result.IsSuccess ?
                    Result.Success(dataView) :
                    Result.Error($"Not able to upload intermediate data to S3 for nodeType: {nodeType}, nodeGuid: {nodeGuid}. Detail: {result.Errors.FirstOrDefault()}");
            }
            catch (Exception ex)
            {
                var message = $"Error when generating intermediate data for nodeType: {nodeType}, nodeGuid: {nodeGuid}";

                _logger.LogError(ex, message);
                return Result.Error(message);
            }
        }
    }
}
