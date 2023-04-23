namespace AIaaS.WebAPI.Models
{
    public class Dataset : AuditableEntity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Delimiter { get; set; }
        public List<ColumnSetting> ColumnSettings { get; set; } = new List<ColumnSetting>();
        public FileStorage? FileStorage { get; set; }
    }
}
