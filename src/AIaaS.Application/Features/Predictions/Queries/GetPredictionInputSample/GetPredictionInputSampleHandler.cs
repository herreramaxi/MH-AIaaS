using AIaaS.Application.Common.Models;
using AIaaS.WebAPI.Interfaces;
using Ardalis.Result;
using MediatR;

namespace AIaaS.Application.Features.Predictions.Queries.GetPredictionInputSample
{
    public class GetPredictionInputSampleHandler : IRequestHandler<GetPredictionInputSampleRequest, Result<object?>>
    {
        private readonly IPredictionBuilderDirector _predictionBuilderDirector;

        public GetPredictionInputSampleHandler(IPredictionBuilderDirector predictionBuilderDirector)
        {
            _predictionBuilderDirector = predictionBuilderDirector;
        }
        public async Task<Result<object?>> Handle(GetPredictionInputSampleRequest request, CancellationToken cancellationToken)
        {
            var parameter = new PredictionParameter(request);
            var result = await _predictionBuilderDirector.BuildInputSampleParameter(parameter);

            if (!result.IsSuccess)
            {
                return Result.Error(result.Errors.FirstOrDefault());
            }

            var runtimeTypeInput = ClassFactory.CreateType(parameter.FeatureColumns);
            var instance = Activator.CreateInstance(runtimeTypeInput);

            return Result.Success(instance);
        }
    }
}
