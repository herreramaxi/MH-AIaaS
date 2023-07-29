using AIaaS.Application.Common.Models;
using AIaaS.Application.Features.Workflows.Commands.ValidateAndSaveWorkflow;
using AIaaS.Application.Interfaces;
using AIaaS.Application.Specifications.Workflows;
using AIaaS.Domain.Entities;
using AIaaS.Domain.Interfaces;
using Ardalis.Result;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIaaS.Application.Features.Workflows.Commands
{
    public class ValidateAndSaveCommandHandler : IRequestHandler<ValidateAndSaveCommand, Result<WorkflowDto>>
    {
        private readonly INodeProcessorService _nodeProcessor;
        private readonly IPublisher _publisher;
        private readonly IRepository<Workflow> _workflowRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<RunWorkflowHandler> _logger;

        public ValidateAndSaveCommandHandler(
            INodeProcessorService nodeProcessor,
            IPublisher publisher,
            IRepository<Workflow> workflowRepository,
            IMapper mapper,
            ILogger<RunWorkflowHandler> logger)
        {
            _nodeProcessor = nodeProcessor;
            _publisher = publisher;
            _workflowRepository = workflowRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<WorkflowDto>> Handle(ValidateAndSaveCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var workflow = await _workflowRepository.FirstOrDefaultAsync(new WorkflowByIdIncludeAllSpec(request.WorkflowDto.Id), cancellationToken);
                if (workflow is null)
                {
                    return Result.NotFound("Workflow not found");
                }

                var context = new WorkflowContext()
                {
                    RunWorkflow = false
                };

                var result = await _nodeProcessor.Run(request.WorkflowDto, context, cancellationToken);

                if (!result.IsSuccess)
                {
                    return result;
                }

                if (string.IsNullOrEmpty(result.Value?.Root))
                {
                    return Result.Error("Serialized workflow is null or empty");
                }

                workflow.UpdateData(result.Value.Root);
                await _workflowRepository.UpdateAsync(workflow, cancellationToken);

                var mapped = _mapper.Map<Workflow, WorkflowDto>(workflow);

                return Result.Success(mapped);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when validating workflow", ex.Message);

                return Result.Error($"Error when validating workflow: {ex.Message}");
            }
        }
    }
}
