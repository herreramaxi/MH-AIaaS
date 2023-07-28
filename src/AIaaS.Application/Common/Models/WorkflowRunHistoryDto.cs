using AIaaS.Domain.Enums;

namespace AIaaS.Application.Common.Models
{
    public class WorkflowRunHistoryDto : AuditableEntityDto
    {
        public int Id { get; set; }
        public int WorkflowId { get; set; }
        public string? WorkflowName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public WorkflowRunStatus Status { get; set; }
        public string StatusHumanized { get; set; }
        public string? Description { get; set; }
        public string? StatusDetail { get; set; }
        public double? TotalMilliseconds { get; set; }
    }
}
