using AIaaS.Domain.Common;
using AIaaS.Domain.Entities.enums;

namespace AIaaS.Domain.Entities
{
    public class MLModel : AuditableEntity
    {
        public int WorkflowId { get; set; }
        public Workflow Workflow { get; set; }
        public long Size { get; private set; }
        public byte[] Data { get; private set; }
        public ModelMetrics? ModelMetrics { get; private set; }
        public MLEndpoint? Endpoint { get; set; }

        public void SetData(MemoryStream stream)
        {
            Data = stream.ToArray();
            Size = stream.Length;
        }

        public void UpdateModelMetrics(MetricTypeEnum metrictType, string metricsSerialized)
        {
            if (ModelMetrics == null)
            {
                ModelMetrics = new ModelMetrics { MLModel = this };
            }

            ModelMetrics.MetricType = metrictType;
            ModelMetrics.Data = metricsSerialized;
        }
    }
}
