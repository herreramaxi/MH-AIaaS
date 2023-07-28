using AIaaS.Domain.Common;
using AIaaS.Domain.Enums;
using AIaaS.Domain.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace AIaaS.Domain.Entities
{
    public class WorkflowRunHistory : AuditableEntity, IAggregateRoot
    {
        public int WorkflowId { get; set; }
        public Workflow Workflow { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public WorkflowRunStatus Status { get; set; }
        public string? Description { get; set; }
        public string? StatusDetail { get; set; }
        public double? TotalMilliseconds { get; set; }
        [NotMapped]
        public string? WorkflowName { get; set; }
        private readonly List<WorkflowNodeRunHistory> _workflowNodeRunHistories = new List<WorkflowNodeRunHistory>();
        public IReadOnlyCollection<WorkflowNodeRunHistory> WorkflowNodeRunHistories => _workflowNodeRunHistories.AsReadOnly();

        public WorkflowNodeRunHistory AddWorkflowNodeRunHistory(WorkflowNodeRunHistory workflowNodeRunHistory)
        {
            _workflowNodeRunHistories.Add(workflowNodeRunHistory);

            return workflowNodeRunHistory;
        }

        public static WorkflowRunHistory CreateAsStart(int workflowId)
        {
            return new WorkflowRunHistory
            {
                WorkflowId = workflowId,
                StartDate = DateTime.UtcNow,
                Status = WorkflowRunStatus.Running
            };
        }

        public void Complete(WorkflowRunStatus workflowRunStatus, string? statusDetail)
        {
            this.EndDate = DateTime.UtcNow;
            this.TotalMilliseconds = ((TimeSpan)(this.EndDate - this.StartDate)).TotalMilliseconds;
            this.Status = workflowRunStatus;
            this.StatusDetail = statusDetail;
        }
    }
}
