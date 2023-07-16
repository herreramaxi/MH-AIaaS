using AIaaS.Application.Common.Models;
using AIaaS.Application.Common.Models.CustomAttributes;
using AIaaS.Domain.Entities.enums;
using Ardalis.Result;
using Microsoft.ML;

namespace AIaaS.Application.Features.Predictions.Queries.Common.Builders
{
    [PredictServiceParameterBuilder(PredictServiceBuilderType.Model, 3)]
    public class ModelBuilder : PredictServiceParameterBuilderAbstract
    {
        public override async Task<Result<PredictionParameter>> Build(PredictionParameter parameter)
        {
            if (parameter.Endpoint.MLModel is null)
            {
                return Result.NotFound("Model not found");
            }

            if (parameter.Endpoint.MLModel.Data?.Any() != true)
            {
                return Result.Error("Model has no data");
            }

            parameter.MLContext = new MLContext();
            using var memStream = new MemoryStream(parameter.Endpoint.MLModel.Data);
            var trainedModel = parameter.MLContext.Model.Load(memStream, out var inputSchema);

            if (trainedModel == null)
            {
                return Result.NotFound("Trained model is null");
            }

            if (inputSchema is null)
            {
                return Result.NotFound("InputSchema from trained model is null");
            }

            parameter.TrainedModel = trainedModel;
            parameter.InputSchema = inputSchema;

            return Result.Success(parameter);
        }
    }
}
