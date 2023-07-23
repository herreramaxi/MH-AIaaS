using Ardalis.Result;
using MediatR;

namespace AIaaS.Application.Features.Predictions.Queries.PredictInputSample
{
    public class GetPredictionRequest: IRequest<Result<object>>
    {
        public int EndpointId { get; }
        public Stream RequestBodyStream { get; }
        public bool OnlyPredictionProperties { get; }
        public bool SkipDisableEndpointValidation { get; }

        public GetPredictionRequest(int endpointId, Stream requestBodyStream, bool onlyPredictionProperties = false, bool skipDisableEndpointValidation = false)
        {
            EndpointId = endpointId;
            RequestBodyStream = requestBodyStream;
            OnlyPredictionProperties = onlyPredictionProperties;
            this.SkipDisableEndpointValidation = skipDisableEndpointValidation;
        }    
    }
}
