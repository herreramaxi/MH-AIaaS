using AIaaS.Application.Common.Models;

namespace AIaaS.Application.Common.Models
{
    public class WorkflowItemDto : AuditableEntityDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
    }
}
