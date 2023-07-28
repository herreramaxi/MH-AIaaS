using AIaaS.Application.Common.Models;
using AIaaS.Application.Specifications.Workflows;
using AIaaS.Domain.Entities;
using AIaaS.Domain.Interfaces;
using AIaaS.WebAPI.Interfaces;
using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.ML;

namespace AIaaS.Application.Features.Workflows.Commands
{
    public class ValidateWorkflowCommand : IRequest<Result<WorkflowDto>>
    {
        public ValidateWorkflowCommand(WorkflowDto workflowDto)
        {
            WorkflowDto = workflowDto;
        }

        public WorkflowDto WorkflowDto { get; }
    }

    public class ValidateWorkflowHandler : IRequestHandler<ValidateWorkflowCommand, Result<WorkflowDto>>
    {
        private readonly IWorkflowService _workflowService;
        private readonly IReadRepository<Workflow> _readRepository;
        private readonly ILogger<RunWorkflowHandler> _logger;

        public ValidateWorkflowHandler(
            IWorkflowService workflowService,
            IReadRepository<Workflow> readRepository,
            ILogger<RunWorkflowHandler> logger)
        {
            _workflowService = workflowService;
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

                var result = await _workflowService.Run(workflowDto, context, cancellationToken);

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
