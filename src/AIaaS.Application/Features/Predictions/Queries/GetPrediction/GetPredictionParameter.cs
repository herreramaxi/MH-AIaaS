namespace AIaaS.Application.Features.Predictions.Queries.PredictInputSample
{
    internal class GetPredictionParameter
    {
        public int EndpointId { get; set; }
        public Stream? RequestBodyStream { get; set; }
        public bool OnlyPredictionProperties { get; set; }
    }
}
