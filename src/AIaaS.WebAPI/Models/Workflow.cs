namespace AIaaS.WebAPI.Models
{
    public class Workflow : AuditableEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description{ get; set; }
        public bool? IsPublished { get; set; }
    }
}
