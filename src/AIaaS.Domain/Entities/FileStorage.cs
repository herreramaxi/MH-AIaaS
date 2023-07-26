using AIaaS.Domain.Common;

namespace AIaaS.Domain.Entities
{
    public class FileStorage : AuditableEntity
    {
        public int DatasetId { get; set; }
        public Dataset Dataset { get; set; } = null!;
        public string FileName { get; set; }
        public long Size { get; set; }
        public string S3Key{ get; set; }
    }
}
