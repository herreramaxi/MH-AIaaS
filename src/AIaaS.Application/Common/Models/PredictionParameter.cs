using AIaaS.Domain.Entities;
using Microsoft.ML;

namespace AIaaS.Application.Common.Models
{
    public class PredictionParameter
    {
        public int EndpointId { get; set; }
        public StreamReader StreamReader { get; set; }
        public MLEndpoint Endpoint { get; set; }
        public MLContext MLContext { get; set; }
        public ITransformer TrainedModel { get; set; }
        public DataViewSchema InputSchema { get; set; }
        public Workflow Workflow { get; set; }
        public string Label { get; set; }
        public DataViewSchema.Column LabelColumn { get; set; }
        public string Task { get; set; }
        public string InputAsString { get; set; }
        public Type RuntimeTypeInput { get; set; }
        public Type RuntimeTypeOutput { get; set; }
        public object RuntimeInstancesInput { get; set; }
        public IEnumerable<(string Name, Type RawType)> FeatureColumns { get; set; }
        public IList<string> SelectedColumns { get; internal set; }
        public bool OnlyPredictedProperties { get; private set; }

        public PredictionParameter(int endpointId)
        {
            this.EndpointId = endpointId;
        }

        public PredictionParameter(int endpointId, StreamReader streamReader, bool onlyPredictedProperties = false)
        {
            this.EndpointId = endpointId;
            this.StreamReader = streamReader;
            this.OnlyPredictedProperties = onlyPredictedProperties;
        }       
    }
}
