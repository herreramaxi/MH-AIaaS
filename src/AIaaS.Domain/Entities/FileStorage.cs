using AIaaS.Domain.Common;

namespace AIaaS.Domain.Entities
{
    public class FileStorage : AuditableEntity
    {
        public int Id { get; set; }
        public int DatasetId { get; set; }
        public Dataset Dataset { get; set; } = null!;
        public string FileName { get; set; }
        public long Size { get; set; }
        public byte[] Data { get; set; } 
    }
}
