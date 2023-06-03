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
                root.Error($"Workflow operator not found for type {root.Type}");
                return;
            }

            try
            {
                await workflowOperator.Execute(context, root);
            }
            catch (Exception ex)
            {
                root.Error($"Error when executing operator {root.Type}");
                return;
            }

            await TraverseTreeDFS(root.Children?.FirstOrDefault(), context);
        }
    }
}
