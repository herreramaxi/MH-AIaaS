using AIaaS.Application.Common.Models;
using AIaaS.Application.Common.Models.Dtos;
using AIaaS.Application.Interfaces;
using AIaaS.WebAPI.ExtensionMethods;
using AIaaS.WebAPI.Interfaces;
using Ardalis.Result;

namespace AIaaS.Application.Services
{
    public class NodeProcessorService : INodeProcessorService
    {
        private readonly IEnumerable<IWorkflowOperator> _workflowOperators;
        private readonly IWorkflowRunHistoryContext _workflowRunHistoryContext;

        // Define delegate types for event handlers
        public delegate Task NodeStartProcessingHandler(WorkflowNodeDto? node, int? workflowRunHistoryId);
        public delegate Task NodeFinishProcessingHandler(WorkflowNodeDto? node, Result processNodeResult);

        // Define events based on the delegate types
        public event NodeStartProcessingHandler? NodeStartProcessingEvent;
        public event NodeFinishProcessingHandler? NodeFinishProcessingEvent;
        public NodeProcessorService(IEnumerable<IWorkflowOperator> workflowOperators, IWorkflowRunHistoryContext workflowRunHistoryContext)
        {
            _workflowOperators = workflowOperators;
            _workflowRunHistoryContext = workflowRunHistoryContext;
        }

        public async Task<Result<WorkflowDto>> Run(WorkflowDto workflowDto, WorkflowContext context, CancellationToken cancellationToken)
        {
            var workflowGraphDto = workflowDto.GetDeserializedWorkflowGraph();

            if (workflowGraphDto is null)
            {
                return Result.Error("Not able to process workflow");
            }

            var nodes = workflowGraphDto.Root.ToList(doubleLinked: true, generateGuidIfNotExist: true);

            if (!nodes.Any())
            {
                return Result.Error("There are no operators in the workflow, please configure your workflow with operators");
            }

            foreach (WorkflowNodeDto node in nodes)
            {
                await OnNodeStartProcessing(node, _workflowRunHistoryContext.WorkflowRunHistory?.Id);

                var result = await ProcessNode(node, context, cancellationToken);

                await OnNodeFinishProcessing(node, _workflowRunHistoryContext.WorkflowRunHistory?.Id, result);
            }

            workflowDto.UpdateSerializedRootFromGraph(workflowGraphDto);

            if (string.IsNullOrEmpty(workflowDto.Root))
            {
                return Result.Error("Serialized workflow is null or empty");
            }

            return Result.Success(workflowDto);
        }

        private async Task OnNodeStartProcessing(WorkflowNodeDto node, int? workflowRunHistoryId)
        {
            node.SetAsRunning();

            if (NodeStartProcessingEvent is null) return;
            await NodeStartProcessingEvent.Invoke(node, workflowRunHistoryId);
        }

        private async Task OnNodeFinishProcessing(WorkflowNodeDto node, int? workflowRunHistoryId, Result processNodeResult)
        {
            if (!processNodeResult.IsSuccess)
            {
                node.SetAsFailed(processNodeResult.Errors.FirstOrDefault() ?? "");
            }
            else
            {
                node.SetAsSuccess();
            }

            if (NodeFinishProcessingEvent is null) return;
            await NodeFinishProcessingEvent.Invoke(node, processNodeResult);
        }

        private async Task<Result> ProcessNode(WorkflowNodeDto node, WorkflowContext context, CancellationToken cancellationToken)
        {
            try
            {
                var workflowOperator = _workflowOperators.FirstOrDefault(x => x.Type.Equals(node.Type, StringComparison.InvariantCultureIgnoreCase));
                if (workflowOperator is null)
                {
                    return Result.Error($"Workflow operator not found for type {node.Type}");
                }

                workflowOperator.Preprocessing(context, node);
                await workflowOperator.Hydrate(context, node);
                workflowOperator.PropagateDatasetColumns(context, node);
                var validationResult = workflowOperator.Validate(context, node);

                if (!validationResult.IsSuccess || !context.RunWorkflow)
                {
                    return validationResult;
                }

                var runResult = await workflowOperator.Run(context, node, cancellationToken);
                if (!runResult.IsSuccess)
                {
                    return runResult;
                }

                var generateOutputresult = await workflowOperator.GenerateOuput(context, node, cancellationToken);

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Error($"Error when executing operator {node.Type}: {ex.Message}");
            }
        }
    }
}
