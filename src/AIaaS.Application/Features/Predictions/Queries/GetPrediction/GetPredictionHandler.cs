using AIaaS.Application.Common.Models;
using AIaaS.WebAPI.ExtensionMethods;
using AIaaS.WebAPI.Interfaces;
using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.ML;
using Microsoft.ML.Data;
using System.Dynamic;

namespace AIaaS.Application.Features.Predictions.Queries.PredictInputSample
{
    public class GetPredictionHandler : IRequestHandler<GetPredictionRequest, Result<object>>
    {
        private readonly IPredictionBuilderDirector _predictionBuilderDirector;
        private readonly ILogger<GetPredictionHandler> _logger;

        public GetPredictionHandler(IPredictionBuilderDirector predictionBuilderDirector, ILogger<GetPredictionHandler> logger)
        {
            _predictionBuilderDirector = predictionBuilderDirector;
            _logger = logger;
        }
        public async Task<Result<object>> Handle(GetPredictionRequest request, CancellationToken cancellationToken)
        {
            try
            {
                using var stream = new StreamReader(request.RequestBodyStream);
                var predictParameter = new PredictionParameter(request.EndpointId, stream, request.OnlyPredictionProperties, request.SkipDisableEndpointValidation);
                var result = await _predictionBuilderDirector.BuildPredictionParameter(predictParameter);

                if (!result.IsSuccess)
                {
                    return Result<object>.Error(result.Errors.ToArray());
                }

                var prediction = this.GeneratePrediction(result.Value);

                if (prediction is null)
                {
                    return Result.Error("Error when trying to predict: prediction is null");
                }

                dynamic dynamicObject = predictParameter.OnlyPredictedProperties ?
                    prediction :
                    this.MergeInputParameterWithPrediction(predictParameter, prediction);

                return Result.Success((object)dynamicObject);
            }
            catch (Exception ex)
            {
                var message = $"Error when trying to generate prediction: {ex.Message}";
                _logger.LogError(ex, message);

                return Result.Error(message);
            }
        }

        private object? GeneratePrediction(PredictionParameter predictParameter)
        {
            var dynamicPredictionEngine = predictParameter.MLContext.Model.InvokeGenericMethod(
                new Type[] { predictParameter.RuntimeTypeInput, predictParameter.RuntimeTypeOutput },
                "CreatePredictionEngine",
                new[] { typeof(ITransformer),
                    typeof(bool), typeof(SchemaDefinition), typeof(SchemaDefinition) },
                new object[] { predictParameter.TrainedModel, true, null, null });

            var predictedObject = dynamicPredictionEngine?.InvokeMethod("Predict", new[] { predictParameter.RuntimeTypeInput }, new[] { predictParameter.RuntimeInstancesInput });
            return predictedObject;
        }

        private dynamic MergeInputParameterWithPrediction(PredictionParameter predictParameter, object? aPrediction)
        {
            dynamic dynamicObject = new ExpandoObject();

            foreach (var prop in predictParameter.RuntimeTypeInput.GetProperties())
            {
                //skip if predictedObject already has the property
                if (predictParameter.RuntimeTypeOutput.GetProperty(prop.Name) is not null)
                {
                    continue;
                }

                var value = prop.GetValue(predictParameter.RuntimeInstancesInput);
                ((IDictionary<string, object>)dynamicObject)[prop.Name] = value;
            }

            foreach (var prop in predictParameter.RuntimeTypeOutput.GetProperties())
            {
                var value = prop.GetValue(aPrediction);
                ((IDictionary<string, object>)dynamicObject)[prop.Name] = value;
            }

            return dynamicObject;
        }
    }
}
