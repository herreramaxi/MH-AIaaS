using AIaaS.Domain.Common;
using AIaaS.Domain.Enums;
using AIaaS.Domain.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

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
        [NotMapped]
        public TimeSpan? Duration => EndDate != null ? EndDate - StartDate : null;
    }
}
