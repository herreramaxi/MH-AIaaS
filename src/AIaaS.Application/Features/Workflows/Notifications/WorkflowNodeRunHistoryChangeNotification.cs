using AIaaS.Domain.Entities;
using MediatR;

namespace AIaaS.Application.Features.Workflows.Notifications
{
    public class WorkflowNodeRunHistoryChangeNotification : INotification
    {
        public WorkflowNodeRunHistoryChangeNotification(WorkflowNodeRunHistory workflowNodeRunHistory)
        {
            WorkflowNodeRunHistory = workflowNodeRunHistory;
        }

        public WorkflowNodeRunHistory WorkflowNodeRunHistory { get; }
    }
}
