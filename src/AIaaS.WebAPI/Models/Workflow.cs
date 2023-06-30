namespace AIaaS.WebAPI.Models
{
    public class Workflow : AuditableEntity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public bool? IsPublished { get; set; }
        public bool? IsModelGenerated { get; set; }
        public MLModel? MLModel { get; set; }
        public string? Data { get; set; }
    }
}
