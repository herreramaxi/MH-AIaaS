using Ardalis.Result;
using MediatR;

namespace AIaaS.Application.Features.Predictions.Queries.GetPredictionInputSample
{
    public class GetPredictionInputSampleRequest: IRequest<Result<object?>>
    {
        public GetPredictionInputSampleRequest(int endpointId)
        {
            EndpointId = endpointId;
        }

        public int EndpointId { get; }
    }
}
