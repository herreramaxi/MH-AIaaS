using AIaaS.Domain.Enums;

namespace AIaaS.Application.Common.Models
{
    public class WorkflowRunHistoryDto
    {
        public int WorkflowId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public WorkflowRunStatus Status { get; set; }
        public string StatusHumanized { get; set; }
        public string? Description { get; set; }
        public string? StatusDetail { get; set; }
        public int? Milliseconds { get; set; }
        public int? Seconds { get; set; }
        public int? Minutes{ get; set; }
    }
}
