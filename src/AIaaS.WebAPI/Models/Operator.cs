namespace AIaaS.WebAPI.Models
{
    public class Operator: AuditableEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description{ get; set; }
    }
}
