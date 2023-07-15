using AIaaS.Application.Common.Models;
using AIaaS.Application.Specifications;
using AIaaS.Application.Workflows.Commands.RunWorkflow;
using AIaaS.Domain.Entities;
using AIaaS.Domain.Interfaces;
using AIaaS.WebAPI.CQRS.Handlers;
using AIaaS.WebAPI.Interfaces;
using Ardalis.Result;
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
        private readonly IReadRepository<Workflow> _readRepository;
        private readonly ILogger<RunWorkflowHandler> _logger;

        public ValidateWorkflowHandler(
            IEnumerable<IWorkflowOperator> workflowOperators,
            IReadRepository<Workflow> readRepository,
            ILogger<RunWorkflowHandler> logger) : base(workflowOperators)
        {
            _readRepository = readRepository;
            _logger = logger;
        }

        public async Task<Result<WorkflowDto>> Handle(ValidateWorkflowCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var workflowDto = request.WorkflowDto;
                var workflow = await _readRepository.FirstOrDefaultAsync(new WorkflowByIdSpec(workflowDto.Id), cancellationToken);

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

                var result = await Run(workflowDto, context, cancellationToken);

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
