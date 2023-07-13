using AIaaS.Domain.Common;
using AIaaS.Domain.Entities.enums;

namespace AIaaS.Domain.Entities
{
    public class ModelMetrics: AuditableEntity
    {
        public int Id { get; set; }
        public int MLModelId { get; set; }
        public MetricTypeEnum MetricType { get; set; }
        public MLModel? MLModel { get; set; }
        public string Data { get; set; } 
    }
}
