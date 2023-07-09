using AIaaS.WebAPI.CQRS.Commands;
using AIaaS.WebAPI.Data;
using AIaaS.WebAPI.Interfaces;
using AIaaS.WebAPI.Models;
using Ardalis.Result;
using MediatR;
using Microsoft.ML;

namespace AIaaS.WebAPI.CQRS.Handlers
{
    public class ValidateWorkflowHandler : BaseWorkflowHandler, IRequestHandler<ValidateWorkflowCommand, Result<WorkflowDto>>
    {
        private readonly EfContext _dbContext;
        private readonly ILogger<RunWorkflowHandler> _logger;

        public ValidateWorkflowHandler(
            IEnumerable<IWorkflowOperator> workflowOperators,
            EfContext dbContext,
            ILogger<RunWorkflowHandler> logger) : base(workflowOperators, dbContext)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<Result<WorkflowDto>> Handle(ValidateWorkflowCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var workflowDto = request.WorkflowDto;
                var workflow = await _dbContext.Workflows.FindAsync(workflowDto.Id);

                if (workflow is null)
                {
                    return Result.NotFound("Workflow not found");
                }

                var context = new WorkflowContext()
                {
                    MLContext = new MLContext(seed: 0),
                    Workflow = workflow,
                    RunWorkflow = false
                };

                var result = await Run(workflowDto, context);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when validating workflow", ex.Message);

                return Result.Error($"Error when validating workflow: {ex.Message}");
            }
        }
    }
}
