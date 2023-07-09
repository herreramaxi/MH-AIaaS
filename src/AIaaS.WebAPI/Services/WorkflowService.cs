using AIaaS.WebAPI.Data;
using AIaaS.WebAPI.ExtensionMethods;
using AIaaS.WebAPI.Interfaces;
using AIaaS.WebAPI.Models;
using AIaaS.WebAPI.Models.Dtos;
using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using System.Text.Json;

namespace AIaaS.WebAPI.Services
{
    public class WorkflowService : IWorkflowService
    {
        private readonly IEnumerable<IWorkflowOperator> _workflowOperators;
        private readonly EfContext _dbContext;
        private readonly ILogger<IWorkflowService> _logger;

        public WorkflowService(IEnumerable<IWorkflowOperator> workflowOperators, EfContext dbContext, ILogger<IWorkflowService> logger)
        {
            _workflowOperators = workflowOperators;
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<Result<WorkflowDto>> Run(WorkflowDto workflowDto)
        {
            try
            {
                var workflow = await _dbContext.Workflows
                    .Include(w => w.WorkflowDataViews)
                    .FirstOrDefaultAsync(w => w.Id == workflowDto.Id);

                if (workflow is null)
                {
                    return Result.NotFound("Workflow not found");
                }

                if (string.IsNullOrEmpty(workflowDto?.Root))
                {
                    return Result.Error("Workflow is required, root property is empty");
                }

                var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var workflowGraphDto = JsonSerializer.Deserialize<WorkflowGraphDto>(workflowDto.Root, jsonOptions);

                if (workflowGraphDto is null)
                {
                    return Result.Error("Not able to process workflow");
                }

                var context = new WorkflowContext()
                {
                    MLContext = new MLContext(seed: 0),
                    Workflow = workflow,
                    RunWorkflow = true
                };

                var nodes = workflowGraphDto.Root.ToList(true);

                foreach (var node in nodes)
                {
                    await this.ProcessNode(node, context);
                }

                var workflowSerialized = JsonSerializer.Serialize(workflowGraphDto, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

                workflow.Data = workflowSerialized;

                _dbContext.Workflows.Update(workflow);
                await _dbContext.SaveChangesAsync();

                workflowDto.Root = workflow.Data;
                workflowDto.ModifiedOn = workflow.ModifiedOn;
                workflowDto.ModifiedBy = workflow.ModifiedBy;
                workflowDto.IsModelGenerated = workflow.MLModel != null;

                return Result.Success(workflowDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when running workflow", ex.Message);

                return Result.Error($"Error when running workflow: {ex.Message}");
            }
        }

        public async Task<Result<WorkflowDto>> Validate(WorkflowDto workflowDto)
        {
            try
            {
                var workflow = await _dbContext.Workflows.FindAsync(workflowDto.Id);

                if (workflow is null)
                {
                    return Result.NotFound("Workflow not found");
                }

                if (workflowDto.Root is null)
                {
                    return Result.Error("Not able to process workflow, root property is empty");
                }

                var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var workflowGraphDto = JsonSerializer.Deserialize<WorkflowGraphDto>(workflowDto.Root, jsonOptions);

                if (workflowGraphDto is null)
                {
                    return Result.Error("Not able to process workflow, deserealization failed");
                }

                var context = new WorkflowContext()
                {
                    MLContext = new MLContext(seed: 0),
                    Workflow = workflow
                };

                var nodes = workflowGraphDto.Root.ToList(true);

                foreach (var node in nodes)
                {
                    await this.ProcessNode(node, context);
                }

                //await TraverseTreeDFS(workflowGraphDto.Root, context);

                workflowDto.Root = JsonSerializer.Serialize(workflowGraphDto, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                return Result.Success(workflowDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when validating workflow", ex.Message);

                return Result.Error($"Error when validating workflow: {ex.Message}");
            }
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



        //private async Task TraverseTreeDFS(WorkflowNodeDto? root, WorkflowContext context)
        //{
        //    if (root is null)
        //        return;

        //    var workflowOperator = _workflowOperators.FirstOrDefault(x => x.Type.Equals(root.Type, StringComparison.InvariantCultureIgnoreCase));
        //    if (workflowOperator is null)
        //    {
        //        root.SetAsFailed($"Workflow operator not found for type {root.Type}");
        //        return;
        //    }

        //    var child = root.Children?.FirstOrDefault();

        //    try
        //    {
        //        workflowOperator.Preprocessing(context, root);
        //        await workflowOperator.Hydrate(context, root);
        //        workflowOperator.PropagateDatasetColumns(context, root);
        //        var isValid = workflowOperator.Validate(context, root);
        //        //workflowOperator.Postprocessing(context, root);
        //        if (context.RunWorkflow && isValid)
        //        {
        //            await workflowOperator.Run(context, root);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        root.SetAsFailed($"Error when executing operator {root.Type}: {ex.Message}");
        //        return;
        //    }           

        //    await TraverseTreeDFS(child, context);
        //}
    }
}
