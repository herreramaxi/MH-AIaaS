using AIaaS.Application.Common.Models;
using AIaaS.Application.Features.Workflows;
using AIaaS.Application.Features.Workflows.Commands;
using AIaaS.Application.Features.Workflows.Notifications;
using AIaaS.Application.Interfaces;
using AIaaS.Domain.Entities;
using AIaaS.Domain.Enums;
using AIaaS.Domain.Interfaces;
using Ardalis.Result;
using MediatR;

namespace AIaaS.Application.Common.Behaviours
{
    public class WorkflowRunHistoryPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : RunWorkflowCommand
        where TResponse : Result<WorkflowDto>
    {
        private readonly IMediator _mediator;
        private readonly IRepository<WorkflowRunHistory> _workflowRunHistoryRepository;
        private readonly IWorkflowRunHistoryContext _workflowRunHistoryContext;

        public WorkflowRunHistoryPipelineBehavior(IMediator mediator,
            IRepository<WorkflowRunHistory> workflowRunHistoryRepository,
            IWorkflowRunHistoryContext workflowRunHistoryContext
            )
        {
            _workflowRunHistoryRepository = workflowRunHistoryRepository;
            _workflowRunHistoryContext = workflowRunHistoryContext;
            _mediator = mediator;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var workflowRunHistory = WorkflowRunHistory.CreateAsStart(request.WorkflowDto.Id);
            await _workflowRunHistoryRepository.AddAsync(workflowRunHistory, cancellationToken);
            _workflowRunHistoryContext.SetWorkflowRunHistory(workflowRunHistory);

            await _mediator.Publish(new WorkflowRunHistoryChangeNotification(workflowRunHistory), cancellationToken);

            //Execute RunWorkflowHandler
            TResponse response = await next();

            workflowRunHistory.Complete(
                response.IsSuccess ? WorkflowRunStatus.Finished : WorkflowRunStatus.Failed,
                response.Errors?.FirstOrDefault());
            await _workflowRunHistoryRepository.UpdateAsync(workflowRunHistory, cancellationToken);
            await _mediator.Publish(new WorkflowRunHistoryChangeNotification(workflowRunHistory), cancellationToken);

            return response;
        }
    }
}
