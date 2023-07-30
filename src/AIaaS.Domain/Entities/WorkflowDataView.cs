using AIaaS.Domain.Common;
using AIaaS.Domain.Interfaces;

namespace AIaaS.Domain.Entities
{
    public class WorkflowDataView : AuditableEntity, IDataViewFile, IAggregateRoot
    {
        public int WorkflowId { get; set; }
        public Workflow Workflow { get; set; } = null!;
        public Guid NodeGuid { get; set; }
        public string NodeType { get; set; }      
        public long Size { get; set; }
        public string S3Key { get; set; }
    }
}
