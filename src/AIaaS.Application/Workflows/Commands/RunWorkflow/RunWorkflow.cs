using AIaaS.Application.Common.Models;
using AIaaS.Domain.Entities;
using AIaaS.WebAPI.CQRS.Handlers;
using AIaaS.WebAPI.Interfaces;
using Ardalis.Result;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.ML;

namespace AIaaS.Application.Workflows.Commands.RunWorkflow
{
    public class RunWorkflowCommand : IRequest<Result<WorkflowDto>>
    {
        public RunWorkflowCommand(WorkflowDto workflowDto)
        {
            WorkflowDto = workflowDto;
        }

        public WorkflowDto WorkflowDto { get; }
    }

    public class RunWorkflowHandler : BaseWorkflowHandler, IRequestHandler<RunWorkflowCommand, Result<WorkflowDto>>
    {
        private readonly IWorkflowService _workflowService;
        private readonly IMapper _mapper;
        private readonly ILogger<RunWorkflowHandler> _logger;

        public RunWorkflowHandler(IEnumerable<IWorkflowOperator> workflowOperators,
           IWorkflowService workflowService,
            IMapper mapper,
            ILogger<RunWorkflowHandler> logger) : base(workflowOperators)
        {
            _workflowService = workflowService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<WorkflowDto>> Handle(RunWorkflowCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var workflow = await _workflowService.WorkflowByIdIncludeAll(request.WorkflowDto.Id, cancellationToken);

                if (workflow is null)
                {
                    return Result.NotFound("Workflow not found");
                }

                var context = new WorkflowContext()
                {
                    MLContext = new MLContext(seed: 0),
                    Workflow = workflow,
                    RunWorkflow = true
                };

                var result = await this.Run(request.WorkflowDto, context, cancellationToken);

                if (!result.IsSuccess)
                {
                    return result;
                }

                if (string.IsNullOrEmpty(result.Value?.Root))
                {
                    return Result.Error("Serialized workflow is null or empty");
                }

                await _workflowService.UpdateWorkflowData(workflow, result.Value.Root, cancellationToken);
                var mapped = _mapper.Map<Workflow, WorkflowDto>(workflow);

                return Result.Success(mapped);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when running workflow", ex.Message);

                return Result.Error($"Error when running workflow: {ex.Message}");
            }

        }
    }
}
