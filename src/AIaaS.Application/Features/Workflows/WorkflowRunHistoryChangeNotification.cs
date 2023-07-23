using AIaaS.Domain.Entities;
using MediatR;

namespace AIaaS.Application.Features.Workflows
{
    public class WorkflowRunHistoryChangeNotification : INotification
    {
        public WorkflowRunHistoryChangeNotification(WorkflowRunHistory workflowRunHistory)
        {
            WorkflowRunHistory = workflowRunHistory;
        }

        public WorkflowRunHistory WorkflowRunHistory { get; }
    }
}
