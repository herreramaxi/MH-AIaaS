using AIaaS.Domain.Enums;

namespace AIaaS.Application.Common.Models
{
    public class WorkflowNodeRunHistoryDto : AuditableEntityDto
    {
        public int Id { get; set; }
        public int WorkflowRunHistoryId { get; set; }
        public Guid NodeGuid { get; set; }
        public string NodeType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public WorkflowRunStatus Status { get; set; }
        public string StatusHumanized { get; set; }
        public string? StatusDetail { get; set; }
        public double? TotalMilliseconds { get; set; }
    }
}
