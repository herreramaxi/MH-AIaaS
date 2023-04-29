using AIaaS.WebAPI.Models.Dtos;

namespace AIaaS.WebAPI.Models
{
    public class WorkflowDto : AuditableEntityDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description{ get; set; }
        public bool? IsPublished { get; set; }
        //public WorkflowGraphDto Root { get; set; }
        public string? Root { get; set; }
    }
}
