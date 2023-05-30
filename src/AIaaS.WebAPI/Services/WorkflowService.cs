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

        public async Task<Result> Run(WorkflowGraphDto workflowGraphDto, Workflow workflow)
        {
            var context = new WorkflowContext()
            {
                MLContext = new MLContext(seed: 0),
                Workflow = workflow
            };

           await TraverseTreeDFS(workflowGraphDto.Root, context);

            return Result.Success();
        }

        private async Task TraverseTreeDFS(WorkflowNodeDto? root, WorkflowContext context)
        {
            if (root is null)
                return;

            var workflowOperator = _workflowOperators.FirstOrDefault(x => x.Type.Equals(root.Type, StringComparison.InvariantCultureIgnoreCase));
            if (workflowOperator is null)
                throw new Exception($"Workflow operator not found for type {root.Type}");

            await workflowOperator.Execute(context, root);

            await TraverseTreeDFS(root.Children?.FirstOrDefault(), context);
        }
    }
}
