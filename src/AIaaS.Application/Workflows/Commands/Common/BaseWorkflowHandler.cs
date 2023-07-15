using AIaaS.Application.Common.Models;
using AIaaS.Application.Common.Models.Dtos;
using AIaaS.WebAPI.ExtensionMethods;
using AIaaS.WebAPI.Interfaces;
using Ardalis.Result;
using CleanArchitecture.Application.Common.Interfaces;
using System.Text.Json;

namespace AIaaS.WebAPI.CQRS.Handlers
{
    public class BaseWorkflowHandler
    {
        private readonly IEnumerable<IWorkflowOperator> _workflowOperators;

        public BaseWorkflowHandler(IEnumerable<IWorkflowOperator> workflowOperators) 
        {
            _workflowOperators = workflowOperators;
        }

        public async Task<Result<WorkflowDto>> Run(WorkflowDto workflowDto, WorkflowContext context, CancellationToken cancellationToken)
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
                await this.ProcessNode(node, context, cancellationToken);
            }

            var workflowSerialized = JsonSerializer.Serialize(workflowGraphDto, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            workflowDto.Root = workflowSerialized;

            return Result.Success(workflowDto);
        }

        private async Task ProcessNode(WorkflowNodeDto node, WorkflowContext context, CancellationToken cancellationToken)
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
                    await workflowOperator.Run(context, node, cancellationToken);
                    //TODO: if all good then generate dataview
                    await workflowOperator.GenerateOuput(context, node, cancellationToken);
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
