using AIaaS.Domain.Common;
using AIaaS.Domain.Interfaces;

namespace AIaaS.Domain.Entities
{
    public class Dataset : AuditableEntity, IAggregateRoot
    {  
        public string Name { get; set; }
        public string? Description { get; set; }
        public string Delimiter { get; set; }
        public bool? MissingRealsAsNaNs { get; set; }
        public List<ColumnSetting> ColumnSettings { get; set; } = new List<ColumnSetting>();
        public FileStorage? FileStorage { get; set; }
        public DataViewFile? DataViewFile { get; set; }
    }
}
