using AIaaS.Application.Common.Models;
using AIaaS.Application.Common.Models.Dtos;
using AIaaS.Application.Features.Workflows.Notifications;
using AIaaS.Application.Interfaces;
using AIaaS.Application.Specifications.Workflows;
using AIaaS.Domain.Entities;
using AIaaS.Domain.Interfaces;
using AIaaS.WebAPI.ExtensionMethods;
using AIaaS.WebAPI.Interfaces;
using Ardalis.Result;
using MediatR;
using System.Text.Json;

namespace AIaaS.WebAPI.Services
{
    public class WorkflowService : IWorkflowService
    {
        private readonly IRepository<Workflow> _workflowRepository;
        private readonly IReadRepository<Workflow> _readRepositoryWorkflow;
        private IPublisher _publisher;
        private IEnumerable<IWorkflowOperator> _workflowOperators;
        private IWorkflowRunHistoryContext _workflowRunHistoryContext;
        private readonly IRepository<WorkflowNodeRunHistory> _workflowNodeRunHistoryRepository;

        public WorkflowService(
        IPublisher publisher,
        IEnumerable<IWorkflowOperator> workflowOperators,
        IWorkflowRunHistoryContext workflowRunHistoryContext,
        IRepository<WorkflowNodeRunHistory> WorkflowNodeRunHistoryRepository,
        IRepository<Workflow> workflowRepository,
        IReadRepository<Workflow> readRepositoryWorkflow)
        {
            _publisher = publisher;
            _workflowOperators = workflowOperators;
            _workflowRunHistoryContext = workflowRunHistoryContext;
            _workflowNodeRunHistoryRepository = WorkflowNodeRunHistoryRepository;
            _workflowRepository = workflowRepository;
            _readRepositoryWorkflow = readRepositoryWorkflow;
        }

        public async Task<Result<WorkflowDto>> Run(WorkflowDto workflowDto, WorkflowContext context, CancellationToken cancellationToken)
        {
            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var workflowGraphDto = JsonSerializer.Deserialize<WorkflowGraphDto>(workflowDto.Root ?? "", jsonOptions);

            if (workflowGraphDto is null)
            {
                return Result.Error("Not able to process workflow");
            }

            var nodes = workflowGraphDto.Root.ToList(true);

            foreach (var node in nodes)
            {
                var nodeRunHistory = WorkflowNodeRunHistory.Create(_workflowRunHistoryContext.WorkflowRunHistory?.Id, node.Id, node.Type);
                await _publisher.Publish(new WorkflowNodeRunHistoryChangeNotification(nodeRunHistory), cancellationToken);

                if (context.RunWorkflow && _workflowRunHistoryContext.WorkflowRunHistory?.Id is not null)
                {
                    await _workflowNodeRunHistoryRepository.AddAsync(nodeRunHistory, cancellationToken);
                }

                var processNodeResult = await ProcessNode(node, context, cancellationToken);

                if (!processNodeResult.IsSuccess)
                {
                    node.SetAsFailed(processNodeResult.Errors.FirstOrDefault() ?? "Error when running node");
                }

                nodeRunHistory.CompleteWorkflowRunHistory(
                    processNodeResult.Status == ResultStatus.Ok ? Domain.Enums.WorkflowRunStatus.Finished : Domain.Enums.WorkflowRunStatus.Failed,
                    processNodeResult.Errors.Any() ? processNodeResult.Errors.FirstOrDefault() : null);
                await _publisher.Publish(new WorkflowNodeRunHistoryChangeNotification(nodeRunHistory), cancellationToken);

                if (context.RunWorkflow && _workflowRunHistoryContext.WorkflowRunHistory?.Id is not null)
                {
                    await _workflowNodeRunHistoryRepository.UpdateAsync(nodeRunHistory, cancellationToken);
                }
            }

            var workflowSerialized = JsonSerializer.Serialize(workflowGraphDto, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            workflowDto.Root = workflowSerialized;

            return Result.Success(workflowDto);
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

        public async Task UpdateWorkflowData(Workflow workflow, string data, CancellationToken cancellationToken)
        {
            workflow.UpdateData(data);
            await _workflowRepository.UpdateAsync(workflow, cancellationToken);
        }

        private async Task<Result> ProcessNode(WorkflowNodeDto node, WorkflowContext context, CancellationToken cancellationToken)
        {
            try
            {
                if (node is null)
                    return Result.Error("Not possible to run the node as it is null");

                var workflowOperator = _workflowOperators.FirstOrDefault(x => x.Type.Equals(node.Type, StringComparison.InvariantCultureIgnoreCase));
                if (workflowOperator is null)
                {
                    return Result.Error($"Workflow operator not found for type {node.Type}");
                }

                workflowOperator.Preprocessing(context, node);
                await workflowOperator.Hydrate(context, node);
                workflowOperator.PropagateDatasetColumns(context, node);
                var isValid = workflowOperator.Validate(context, node);

                if (context.RunWorkflow && isValid)
                {
                    //TODO: improve error handling, operator should return success or not, and root.SetAsFailed in another place
                    await workflowOperator.Run(context, node, cancellationToken);
                    //TODO: if all good then generate dataview
                    await workflowOperator.GenerateOuput(context, node, cancellationToken);
                }

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Error($"Error when executing operator {node.Type}: {ex.Message}");
            }
        }
    }
}
