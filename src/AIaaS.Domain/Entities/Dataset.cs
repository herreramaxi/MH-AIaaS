using AIaaS.Domain.Common;

namespace AIaaS.Domain.Entities
{
    public class Dataset : AuditableEntity
    {
        public int Id { get; set; }      
        public string Name { get; set; }
        public string? Description { get; set; }
        public string Delimiter { get; set; }
        public bool? MissingRealsAsNaNs { get; set; }
        public List<ColumnSetting> ColumnSettings { get; set; } = new List<ColumnSetting>();
        public FileStorage? FileStorage { get; set; }
        public DataViewFile? DataViewFile { get; set; }
    }
}
