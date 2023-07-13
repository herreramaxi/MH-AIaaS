using AIaaS.Domain.Common;

namespace AIaaS.Domain.Entities
{
    public class Workflow : AuditableEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public MLModel? MLModel { get; set; }
        public string? Data { get; set; }
        public List<WorkflowDataView> WorkflowDataViews { get; set; } = new List<WorkflowDataView>();
    }
}
