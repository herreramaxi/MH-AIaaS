using AIaaS.Domain.Common;

namespace AIaaS.Domain.Entities
{
    public class DataViewFile: AuditableEntity
    {
        public int DatasetId { get; set; }
        public Dataset Dataset { get; set; } = null!;
        public long Size { get; set; }
        public string? Name { get; set; }
        public string S3Key{ get; set; }
    }
}
