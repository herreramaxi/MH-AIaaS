using AIaaS.Application.Common.Models;
using AIaaS.Application.Common.Models.CustomAttributes;
using AIaaS.Domain.Entities.enums;
using Ardalis.Result;
using System.Text.Json;

namespace AIaaS.WebAPI.Services.PredictionService.Builders
{
    [PredictServiceParameterBuilder(PredictServiceBuilderType.InputParameter, 1)]
    public class InputParameterBuilder : PredictServiceParameterBuilderAbstract
    {
        public override async Task<Result<PredictionParameter>> Build(PredictionParameter parameter)
        {
            var inputAsString = await parameter.StreamReader.ReadToEndAsync();

            if (string.IsNullOrEmpty(inputAsString))
            {
                return Result.Error("Input prediction empty");
            }

            if (!this.IsJson(inputAsString))
            {
                return Result.Error("Input prediction does not have a valid JSON format");
            }

            parameter.InputAsString = inputAsString;

            return Result.Success(parameter);
        }

        public bool IsJson(string jsonString)
        {
            try
            {
                JsonDocument.Parse(jsonString);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}
