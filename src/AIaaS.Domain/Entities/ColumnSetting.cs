using AIaaS.Domain.Common;

namespace AIaaS.Domain.Entities
{
    public class ColumnSetting : AuditableEntity
    {
        public int Id { get; set; }
        public int DatasetId { get; set; }
        public Dataset Dataset { get; set; }
        public string ColumnName { get; set; }
        public bool Include { get; set; }
        //"text" | "numeric" | "boolean" | "date"
        public string Type { get; set; }
    }

}
