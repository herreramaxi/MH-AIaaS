namespace AIaaS.WebAPI.Models
{
    public class WorkflowItemDto : AuditableEntityDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
    }
}
