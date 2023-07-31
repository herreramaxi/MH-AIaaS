using AIaaS.Application.Common.Models;
using AIaaS.Application.Features.Workflows.Commands.ValidateAndSaveWorkflow;
using AIaaS.Application.Features.Workflows.Notifications;
using AIaaS.Application.Interfaces;
using AIaaS.Application.Specifications.Workflows;
using AIaaS.Domain.Entities;
using AIaaS.Domain.Interfaces;
using Ardalis.Result;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIaaS.Application.Features.Workflows.Commands
{
    public class ValidateAndSaveCommandHandler : IRequestHandler<ValidateAndSaveCommand, Result<WorkflowDto>>
    {
        private readonly INodeProcessorService _nodeProcessor;
        private readonly IPublisher _publisher;
        private readonly IRepository<Workflow> _workflowRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<RunWorkflowHandler> _logger;

        public ValidateAndSaveCommandHandler(
            INodeProcessorService nodeProcessor,
            IPublisher publisher,
            IRepository<Workflow> workflowRepository,
            IMapper mapper,
            ILogger<RunWorkflowHandler> logger)
        {
            _nodeProcessor = nodeProcessor;
            _publisher = publisher;
            _workflowRepository = workflowRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<WorkflowDto>> Handle(ValidateAndSaveCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var workflow = await _workflowRepository.FirstOrDefaultAsync(new WorkflowByIdIncludeAllSpec(request.WorkflowDto.Id), cancellationToken);
                if (workflow is null)
                {
                    return Result.NotFound("Workflow not found");
                }
                               
                _nodeProcessor.NodeStartProcessingEvent += (node, workflowRunHistoryId) => _nodeProcessor_NodeStartProcessingEvent(node, workflowRunHistoryId, cancellationToken);
                _nodeProcessor.NodeFinishProcessingEvent += (node, result) => _nodeProcessor_NodeFinishProcessingEvent(node, result, cancellationToken);

                var context = new WorkflowContext() { RunWorkflow = false };
                var result = await _nodeProcessor.Run(request.WorkflowDto, context, cancellationToken, ignoreNodeErrors: true);

                if (result.IsSuccess && !string.IsNullOrEmpty(result.Value?.Root))
                {
                    workflow.UpdateData(result.Value.Root);
                    await _workflowRepository.UpdateAsync(workflow, cancellationToken);
                }
                  
                var mapped = _mapper.Map<Workflow, WorkflowDto>(workflow);

                return result.IsSuccess ? Result.Success(mapped) : result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when validating workflow", ex.Message);

                return Result.Error($"Error when validating workflow: {ex.Message}");
            }
            finally
            {
                _nodeProcessor.NodeStartProcessingEvent -= (node, workflowRunHistoryId) => _nodeProcessor_NodeStartProcessingEvent(node, workflowRunHistoryId, cancellationToken);
                _nodeProcessor.NodeFinishProcessingEvent -= (node, result) => _nodeProcessor_NodeFinishProcessingEvent(node, result, cancellationToken);
            }
        }

        private WorkflowNodeRunHistory? _workflowNodeRunHistory;
        private async Task _nodeProcessor_NodeStartProcessingEvent(Application.Common.Models.Dtos.WorkflowNodeDto? node, int? workflowRunHistoryId, CancellationToken cancellationToken)
        {
            if (node is null || node.Data.NodeGuid is null) return;

            _workflowNodeRunHistory = WorkflowNodeRunHistory.Create(workflowRunHistoryId, node.Data.NodeGuid.Value, node.Type);
            var change = new WorkflowNodeRunHistoryChangeNotification(_workflowNodeRunHistory.NodeGuid,
                _workflowNodeRunHistory.NodeType,
                _workflowNodeRunHistory.Status,
                _workflowNodeRunHistory.StatusDetail);

            await _publisher.Publish(change, cancellationToken);
        }

        private async Task _nodeProcessor_NodeFinishProcessingEvent(Application.Common.Models.Dtos.WorkflowNodeDto? node, Result processNodeResult, CancellationToken cancellationToken)
        {
            if (_workflowNodeRunHistory is null || node is null) return;

            _workflowNodeRunHistory.CompleteWorkflowRunHistory(
               processNodeResult.Status == ResultStatus.Ok ? Domain.Enums.WorkflowRunStatus.Finished : Domain.Enums.WorkflowRunStatus.Failed,
               processNodeResult.Errors.Any() ? processNodeResult.Errors.FirstOrDefault() : null);

            var change = new WorkflowNodeRunHistoryChangeNotification(_workflowNodeRunHistory.NodeGuid,
                _workflowNodeRunHistory.NodeType,
                _workflowNodeRunHistory.Status,
                _workflowNodeRunHistory.StatusDetail,
                node.Data.DatasetColumns,
                nodeParameters: node.Data.Parameters);
            await _publisher.Publish(change, cancellationToken);
        }
    }
}
