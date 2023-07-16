using AIaaS.Application.Common.Models;
using AIaaS.Application.Common.Models.CustomAttributes;
using AIaaS.Domain.Entities.enums;
using AIaaS.WebAPI.ExtensionMethods;
using Ardalis.Result;
using System.Text.Json;

namespace AIaaS.Application.Features.Predictions.Queries.Common.Builders
{
    [PredictServiceParameterBuilder(PredictServiceBuilderType.InputParameterDeserealizer, 6)]
    public class InputParameterDeserealizerBuilder : PredictServiceParameterBuilderAbstract
    {
        public override async Task<Result<PredictionParameter>> Build(PredictionParameter parameter)
        {
            try
            {
                var runtimeInstance = this.InvokeGenericMethod(parameter.RuntimeTypeInput, "DeserealizeObject", new Type[] { typeof(string) }, new object[] { parameter.InputAsString });

                if (runtimeInstance is null)
                {
                    return Result.Error("Error when trying to deserealize input prediction parameter: RuntimeInstance is null");
                }

                parameter.RuntimeInstancesInput = runtimeInstance;

                return Result.Success(parameter);
            }
            catch (Exception ex)
            {
                return Result.Error($"Error when trying to deserealize input prediction parameter from JSON: {ex.Message}");
            }
        }

        public T? DeserealizeObject<T>(string value)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            T? data = JsonSerializer.Deserialize<T>(value, options);
            return data;
        }
    }
}
