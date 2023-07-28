using AIaaS.Domain.Common;
using AIaaS.Domain.Enums;
using AIaaS.Domain.Interfaces;

namespace AIaaS.Domain.Entities
{
    public class WorkflowNodeRunHistory : AuditableEntity, IAggregateRoot
    {
        public int WorkflowRunHistoryId { get; set; }
        public WorkflowRunHistory WorkflowRunHistory { get; set; }
        public string NodeId { get; set; }
        public string NodeType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public WorkflowRunStatus Status { get; set; }
        public string? StatusDetail { get; set; }
        public double? TotalMilliseconds { get; set; }

        public static WorkflowNodeRunHistory Create(int? workflowRunHistoryId,string nodeId, string nodeType) {
            return new WorkflowNodeRunHistory()
            {
                WorkflowRunHistoryId = workflowRunHistoryId??0,
                NodeId = nodeId,
                NodeType = nodeType,
                Status = WorkflowRunStatus.Running,
                StartDate = DateTime.UtcNow
            };
        }
        public void CompleteWorkflowRunHistory(WorkflowRunStatus status, string? statusDetail)
        {          
            this.Status = status;
            this.StatusDetail = statusDetail;
            this.EndDate = DateTime.UtcNow;
            this.TotalMilliseconds = ((TimeSpan)(this.EndDate - this.StartDate)).TotalMilliseconds;
        }
    }
}
