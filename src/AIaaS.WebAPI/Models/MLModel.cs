namespace AIaaS.WebAPI.Models
{
    public class MLModel : AuditableEntity
    {
        public int Id { get; set; }
        public int WorkflowId { get; set; }
        public Workflow? Workflow { get; set; }
        public long Size { get; set; }
        public byte[] Data { get; set; }
        public ModelMetrics? ModelMetrics { get; set; }
    }
}
