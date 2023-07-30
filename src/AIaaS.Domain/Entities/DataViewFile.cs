using AIaaS.Domain.Common;
using AIaaS.Domain.Interfaces;

namespace AIaaS.Domain.Entities
{
    public class DataViewFile: AuditableEntity, IDataViewFile
    {
        public int DatasetId { get; set; }
        public Dataset Dataset { get; set; } = null!;
        public long Size { get; set; }
        public string? Name { get; set; }
        public string S3Key{ get; set; }
    }
}
