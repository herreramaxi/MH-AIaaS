namespace AIaaS.WebAPI.Models
{
    public class User: AuditableEntity
    {
        public int Id{ get; set; }
        public string Email { get; set; }
        //public string Role { get; set; }
        public List<Dataset> Datasets { get; set; } = new List<Dataset>();
        public List<Workflow> Workflows { get; set; } = new List<Workflow>();
    }
}
