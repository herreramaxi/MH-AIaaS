using AIaaS.Application.Features.Predictions.Queries.GetPredictionInputSample;
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
        public IList<string> SelectedColumns { get; set; }
        public bool OnlyPredictedProperties { get; set; }
        public bool SkipDisableEndpointValidation { get; }

        public PredictionParameter(int endpointId, StreamReader streamReader, bool onlyPredictedProperties = false, bool skipDisableEndpointValidation = false)
        {
            this.EndpointId = endpointId;
            this.StreamReader = streamReader;
            this.OnlyPredictedProperties = onlyPredictedProperties;
            this.SkipDisableEndpointValidation = skipDisableEndpointValidation;
        }

        public PredictionParameter(GetPredictionInputSampleRequest request)
        {
            this.EndpointId = request.EndpointId;
            this.SkipDisableEndpointValidation = true;
        }
    }
}
