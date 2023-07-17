using AIaaS.Application.Common.Models;
using AIaaS.Application.Features.Workflows;
using AIaaS.Application.Features.Workflows.Commands;
using AIaaS.Application.Specifications.Workflows;
using AIaaS.Domain.Entities;
using AIaaS.Domain.Enums;
using AIaaS.Domain.Interfaces;
using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIaaS.Application.Common.Behaviours
{
    public class WorkflowRunHistoryPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : RunWorkflowCommand
        where TResponse : Result<WorkflowDto>
    {
        private readonly ILogger<WorkflowRunHistoryPipelineBehavior<TRequest, TResponse>> _logger;
        private readonly IRepository<Workflow> _workflowRepository;
        private readonly IMediator _mediator;

        public WorkflowRunHistoryPipelineBehavior(ILogger<WorkflowRunHistoryPipelineBehavior<TRequest, TResponse>> logger, IRepository<Workflow> workflowRepository, IMediator mediator)
        {
            _logger = logger;
            _workflowRepository = workflowRepository;
            _mediator = mediator;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            DateTime startDate = DateTime.UtcNow;

            var workflow = await _workflowRepository.FirstOrDefaultAsync(new WorkflowByIdSpec(request.WorkflowDto.Id));
            var workflowRunHistory = workflow?.AddWorkflowRunHistory(WorkflowRunStatus.Running, startDate);

            if (workflow is not null && workflowRunHistory is not null)
            {
                _logger.LogInformation($"Running workflow {request.WorkflowDto.Id}-{request.WorkflowDto.Name}, startDate: {startDate}");
                await _workflowRepository.UpdateAsync(workflow, cancellationToken);
                await _mediator.Send(new NotifyWorkflowRunHistoryChangeRequest(workflowRunHistory));
            }

            TResponse response = await next();

            if (workflow is null || workflowRunHistory is null)
            {
                return response;
            }

            workflowRunHistory.EndDate = DateTime.UtcNow;
            workflowRunHistory.Status = response.IsSuccess ? WorkflowRunStatus.Finished : WorkflowRunStatus.Failed;
            workflowRunHistory.StatusDetail = !response.IsSuccess ? response.Errors?.FirstOrDefault() : null;

            await _workflowRepository.UpdateAsync(workflow, cancellationToken);
            await _mediator.Send(new NotifyWorkflowRunHistoryChangeRequest(workflowRunHistory));

            _logger.LogInformation($"Finishing workflow {request.WorkflowDto.Id}-{request.WorkflowDto.Name}, startDate: {startDate}, endDate: {workflowRunHistory.EndDate}, duration: {workflowRunHistory.Duration?.TotalSeconds}s ({workflowRunHistory.Duration?.TotalMilliseconds}ms)");

            return response;
        }
    }
}
