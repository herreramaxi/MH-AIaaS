using AIaaS.WebAPI.Data;
using AIaaS.WebAPI.Interfaces;
using AIaaS.WebAPI.Models;
using AIaaS.WebAPI.Models.Dtos;
using Ardalis.Result;
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

        public async Task<Result<WorkflowDto>> Save(WorkflowDto workflowDto)
        {
            if (workflowDto == null)
                return Result.Error("Workflow is required");

            if (workflowDto.Id <= 0)
                return Result.Error("Workflow id must be greater than zero");

            var workflow = await _dbContext.Workflows.FindAsync(workflowDto.Id);
            if (workflow == null)
                return Result.NotFound();

            workflow.Data = workflowDto.Root;

            _dbContext.Workflows.Update(workflow);
            await _dbContext.SaveChangesAsync();

            workflowDto.ModifiedOn = workflow.ModifiedOn;
            workflowDto.ModifiedBy = workflow.ModifiedBy;

            return Result.Success(workflowDto);
        }

        public async Task<Result<WorkflowDto>> Run(WorkflowDto workflowDto)
        {
            try
            {
                var workflow = await _dbContext.Workflows.FindAsync(workflowDto.Id);

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

                await TraverseTreeDFS(workflowGraphDto.Root, context);

                workflowDto.Root = JsonSerializer.Serialize(workflowGraphDto, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

                workflow.Data = workflowDto.Root;

                _dbContext.Workflows.Update(workflow);
                await _dbContext.SaveChangesAsync();

                workflowDto.ModifiedOn = workflow.ModifiedOn;
                workflowDto.ModifiedBy = workflow.ModifiedBy;

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

                await TraverseTreeDFS(workflowGraphDto.Root, context);

                workflowDto.Root = JsonSerializer.Serialize(workflowGraphDto, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                return Result.Success(workflowDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when validating workflow", ex.Message);

                return Result.Error($"Error when validating workflow: {ex.Message}");
            }
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
                workflowOperator.Preprocessing(context, root, child);
                await workflowOperator.Hydrate(context, root);
                workflowOperator.PropagateDatasetColumns(context, root);
                var isValid = workflowOperator.Validate(context, root);
                workflowOperator.Postprocessing(context, root);

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
