using AIaaS.WebAPI.Interfaces;
using AIaaS.WebAPI.Models;
using AIaaS.WebAPI.Models.Dtos;
using Ardalis.Result;
using Microsoft.ML;

namespace AIaaS.WebAPI.Services
{
    public class WorkflowService : IWorkflowService
    {
        private readonly IEnumerable<IWorkflowOperator> _workflowOperators;

        public WorkflowService(IEnumerable<IWorkflowOperator> workflowOperators)
        {
            _workflowOperators = workflowOperators;
        }

        public async Task<Result<WorkflowGraphDto>> Run(WorkflowGraphDto workflowGraphDto, Workflow workflow)
        {
            var context = new WorkflowContext()
            {
                MLContext = new MLContext(seed: 0),
                Workflow = workflow,
                RunWorkflow = true
            };

            await TraverseTreeDFS(workflowGraphDto.Root, context);

            return Result.Success(workflowGraphDto);
        }

        public async Task<Result<WorkflowGraphDto>> Validate(WorkflowGraphDto workflowGraphDto, Workflow workflow)
        {
            var context = new WorkflowContext()
            {
                MLContext = new MLContext(seed: 0),
                Workflow = workflow
            };

            await TraverseTreeDFS(workflowGraphDto.Root, context);

            return Result.Success(workflowGraphDto);
        }

        private async Task TraverseTreeDFS(WorkflowNodeDto? root, WorkflowContext context)
        {
            if (root is null)
                return;

            var workflowOperator = _workflowOperators.FirstOrDefault(x => x.Type.Equals(root.Type, StringComparison.InvariantCultureIgnoreCase));
            if (workflowOperator is null)
            {
                root.SetAsFailed($"Workflow operator not found for type {root.Type}");
                return;
            }

            var child = root.Children?.FirstOrDefault();

            try
            {
                workflowOperator.Preprocessing(context, root);
                await workflowOperator.Hydrate(context, root);
                workflowOperator.PropagateDatasetColumns(context, root, child);
                var isValid = workflowOperator.Validate(context, root);

                if (context.RunWorkflow && isValid)
                {
                    await workflowOperator.Run(context, root);
                }
            }
            catch (Exception ex)
            {
                root.SetAsFailed($"Error when executing operator {root.Type}");
                return;
            }

            await TraverseTreeDFS(child, context);
        }
    }
}
