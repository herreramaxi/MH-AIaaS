using AIaaS.Application.Common.Models;
using AIaaS.Application.Features.Workflows.Notifications;
using AIaaS.Application.Interfaces;
using AIaaS.Application.Specifications.Workflows;
using AIaaS.Domain.Entities;
using AIaaS.Domain.Interfaces;
using AIaaS.WebAPI.Interfaces;
using Ardalis.Result;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.ML;

namespace AIaaS.Application.Features.Workflows.Commands
{
    public class RunWorkflowCommand : IRequest<Result<WorkflowDto>>
    {
        public RunWorkflowCommand(WorkflowDto workflowDto)
        {
            WorkflowDto = workflowDto;
        }

        public WorkflowDto WorkflowDto { get; }
    }

    public class RunWorkflowHandler : IRequestHandler<RunWorkflowCommand, Result<WorkflowDto>>
    {
        private readonly INodeProcessorService _nodeProcessor;
        private readonly IMapper _mapper;
        private readonly IPublisher _publisher;
        private readonly IRepository<WorkflowNodeRunHistory> _workflowNodeRunHistoryRepository;
        private readonly IRepository<Workflow> _workflowRepository;
        private readonly ILogger<RunWorkflowHandler> _logger;

        public RunWorkflowHandler(
            INodeProcessorService nodeProcessor,
            IMapper mapper,
            IPublisher publisher,
            IRepository<WorkflowNodeRunHistory> workflowNodeRunHistoryRepository,
            IRepository<Workflow> workflowRepository,
            ILogger<RunWorkflowHandler> logger)
        {
            _nodeProcessor = nodeProcessor;
            _mapper = mapper;
            _publisher = publisher;
            _workflowNodeRunHistoryRepository = workflowNodeRunHistoryRepository;
            _workflowRepository = workflowRepository;
            _logger = logger;
        }

        public async Task<Result<WorkflowDto>> Handle(RunWorkflowCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var workflow = await _workflowRepository.FirstOrDefaultAsync(new WorkflowByIdIncludeAllSpec(request.WorkflowDto.Id), cancellationToken);
                if (workflow is null)
                {
                    return Result.NotFound("Workflow not found");
                }

                var context = new WorkflowContext()
                {
                    MLContext = new MLContext(seed: 0),
                    Workflow = workflow,
                    RunWorkflow = true
                };

                _nodeProcessor.NodeStartProcessingEvent += (node, workflowRunHistoryId) => _nodeProcessor_NodeStartProcessingEvent(node, workflowRunHistoryId, cancellationToken);
                _nodeProcessor.NodeFinishProcessingEvent += (node, result) => _nodeProcessor_NodeFinishProcessingEvent(node, result, cancellationToken);

                var result = await _nodeProcessor.Run(request.WorkflowDto, context, cancellationToken);

                if (!result.IsSuccess)
                {
                    return result;
                }

                workflow.UpdateData(result.Value.Root);
                await _workflowRepository.UpdateAsync(workflow, cancellationToken);

                var mapped = _mapper.Map<Workflow, WorkflowDto>(workflow);

                return Result.Success(mapped);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when running workflow", ex.Message);

                return Result.Error($"Error when running workflow: {ex.Message}");
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
            if (workflowRunHistoryId is null || node is null || node.Data.NodeGuid is null) return;

            _workflowNodeRunHistory = WorkflowNodeRunHistory.Create(workflowRunHistoryId, node.Data.NodeGuid.Value, node.Type);
            await _workflowNodeRunHistoryRepository.AddAsync(_workflowNodeRunHistory, cancellationToken);

            var change = new WorkflowNodeRunHistoryChangeNotification(_workflowNodeRunHistory.NodeGuid,
                _workflowNodeRunHistory.NodeType,
                _workflowNodeRunHistory.Status,
                _workflowNodeRunHistory.StatusDetail
                );

            await _publisher.Publish(change, cancellationToken);
        }

        private async Task _nodeProcessor_NodeFinishProcessingEvent(Application.Common.Models.Dtos.WorkflowNodeDto? node, Result processNodeResult, CancellationToken cancellationToken)
        {
            if (_workflowNodeRunHistory is null || node is null) return;

            _workflowNodeRunHistory.CompleteWorkflowRunHistory(
               processNodeResult.Status == ResultStatus.Ok ? Domain.Enums.WorkflowRunStatus.Finished : Domain.Enums.WorkflowRunStatus.Failed,
               processNodeResult.Errors.Any() ? processNodeResult.Errors.FirstOrDefault() : null);

            await _workflowNodeRunHistoryRepository.UpdateAsync(_workflowNodeRunHistory, cancellationToken);

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
