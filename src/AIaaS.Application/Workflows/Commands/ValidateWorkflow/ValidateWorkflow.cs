using AIaaS.Application.Common.Models;
using AIaaS.Application.Workflows.Commands.RunWorkflow;
using AIaaS.WebAPI.CQRS.Handlers;
using AIaaS.WebAPI.Interfaces;
using Ardalis.Result;
using CleanArchitecture.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.ML;

namespace AIaaS.Application.Workflows.Commands.ValidateWorkflow
{
    public class ValidateWorkflowCommand : IRequest<Result<WorkflowDto>>
    {
        public ValidateWorkflowCommand(WorkflowDto workflowDto)
        {
            WorkflowDto = workflowDto;
        }

        public WorkflowDto WorkflowDto { get; }
    }

    public class ValidateWorkflowHandler : BaseWorkflowHandler, IRequestHandler<ValidateWorkflowCommand, Result<WorkflowDto>>
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly ILogger<RunWorkflowHandler> _logger;

        public ValidateWorkflowHandler(
            IEnumerable<IWorkflowOperator> workflowOperators,
            IApplicationDbContext dbContext,
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
