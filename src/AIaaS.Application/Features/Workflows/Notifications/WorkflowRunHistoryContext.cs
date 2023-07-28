using AIaaS.Application.Interfaces;
using AIaaS.Domain.Entities;

namespace AIaaS.Application.Features.Workflows.Notifications
{
    public class WorkflowRunHistoryContext : IWorkflowRunHistoryContext
    {
        public WorkflowRunHistory? WorkflowRunHistory { get; private set; }

        public void SetWorkflowRunHistory(WorkflowRunHistory workflowRunHistory)
        {
            this.WorkflowRunHistory = workflowRunHistory;
        }
    }
}
