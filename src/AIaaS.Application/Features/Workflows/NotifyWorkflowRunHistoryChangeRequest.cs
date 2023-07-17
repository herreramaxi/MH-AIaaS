using AIaaS.Application.Common.Models;
using AIaaS.Domain.Entities;
using MediatR;

namespace AIaaS.Application.Features.Workflows
{
    public class NotifyWorkflowRunHistoryChangeRequest : IRequest<WorkflowRunHistoryDto>
    {
        public NotifyWorkflowRunHistoryChangeRequest(WorkflowRunHistory workflowRunHistory)
        {
            WorkflowRunHistory = workflowRunHistory;
        }

        public WorkflowRunHistory WorkflowRunHistory { get; }
    }
}
