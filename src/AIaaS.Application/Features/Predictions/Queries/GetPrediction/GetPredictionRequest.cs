using Ardalis.Result;
using MediatR;

namespace AIaaS.Application.Features.Predictions.Queries.PredictInputSample
{
    public class GetPredictionRequest: IRequest<Result<object>>
    {
        public GetPredictionRequest(int endpointId, Stream requestBodyStream, bool onlyPredictionProperties = false)
        {
            EndpointId = endpointId;
            RequestBodyStream = requestBodyStream;
            OnlyPredictionProperties = onlyPredictionProperties;
        }
        public int EndpointId { get; }
        public Stream RequestBodyStream { get; }
        public bool OnlyPredictionProperties { get; }
    }
}
