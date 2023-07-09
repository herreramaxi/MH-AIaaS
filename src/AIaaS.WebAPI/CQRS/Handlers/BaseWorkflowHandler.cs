using AIaaS.WebAPI.Data;
using AIaaS.WebAPI.ExtensionMethods;
using AIaaS.WebAPI.Interfaces;
using AIaaS.WebAPI.Models;
using AIaaS.WebAPI.Models.Dtos;
using Ardalis.Result;
using Microsoft.ML;
using System.Text.Json;

namespace AIaaS.WebAPI.CQRS.Handlers
{
    public class BaseWorkflowHandler
    {
        private readonly IEnumerable<IWorkflowOperator> _workflowOperators;
        private readonly EfContext _dbContext;

        public BaseWorkflowHandler(IEnumerable<IWorkflowOperator> workflowOperators, EfContext efContext) 
        {
            _workflowOperators = workflowOperators;
            _dbContext = efContext;
        }

        public async Task<Result<WorkflowDto>> Run(WorkflowDto workflowDto, WorkflowContext context)
        {
            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var workflowGraphDto = JsonSerializer.Deserialize<WorkflowGraphDto>(workflowDto.Root, jsonOptions);

            if (workflowGraphDto is null)
            {
                return Result.Error("Not able to process workflow");
            }

            var nodes = workflowGraphDto.Root.ToList(true);

            foreach (var node in nodes)
            {
                await this.ProcessNode(node, context);
            }

            var workflowSerialized = JsonSerializer.Serialize(workflowGraphDto, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            workflowDto.Root = workflowSerialized;

            return Result.Success(workflowDto);
        }

        private async Task ProcessNode(WorkflowNodeDto node, WorkflowContext context)
        {
            if (node is null)
                return;

            var workflowOperator = _workflowOperators.FirstOrDefault(x => x.Type.Equals(node.Type, StringComparison.InvariantCultureIgnoreCase));
            if (workflowOperator is null)
            {
                node.SetAsFailed($"Workflow operator not found for type {node.Type}");
                return;
            }

            try
            {
                workflowOperator.Preprocessing(context, node);
                await workflowOperator.Hydrate(context, node);
                workflowOperator.PropagateDatasetColumns(context, node);
                var isValid = workflowOperator.Validate(context, node);

                if (context.RunWorkflow && isValid)
                {
                    //TODO: improve error handling, operator should return success or not, and root.SetAsFailed in another place
                    await workflowOperator.Run(context, node);
                    //TODO: if all good then generate dataview
                    await workflowOperator.GenerateOuput(context, node, _dbContext);
                }
            }
            catch (Exception ex)
            {
                node.SetAsFailed($"Error when executing operator {node.Type}: {ex.Message}");
                return;
            }
        }


    }
}
