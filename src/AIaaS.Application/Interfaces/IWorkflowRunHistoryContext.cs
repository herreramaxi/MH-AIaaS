using AIaaS.Domain.Entities;

namespace AIaaS.Application.Interfaces
{
    public interface IWorkflowRunHistoryContext
    {
        WorkflowRunHistory? WorkflowRunHistory { get; }
        void SetWorkflowRunHistory(WorkflowRunHistory workflowRunHistory);
    }
}
