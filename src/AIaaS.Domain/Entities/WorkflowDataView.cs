using AIaaS.Domain.Common;

namespace AIaaS.Domain.Entities
{
    public class WorkflowDataView : AuditableEntity
    {
        public int Id { get; set; }
        public int WorkflowId { get; set; }
        public Workflow Workflow { get; set; } = null!;
        public string NodeId { get; set; }
        public long Size { get; set; }
        public byte[] Data { get; set; }
        public string NodeType { get;  set; }
    }
}
