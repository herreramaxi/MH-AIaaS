using AIaaS.WebAPI.ExtensionMethods;
using AIaaS.WebAPI.Models.CustomAttributes;
using AIaaS.WebAPI.Models.enums;
using Ardalis.Result;

namespace AIaaS.WebAPI.Services.PredictionService.Builders
{
    [PredictServiceParameterBuilder(PredictServiceBuilderType.FeatureColumns, 7)]
    public class FeatureColumnsBuilder : PredictServiceParameterBuilderAbstract
    {
        public override async Task<Result<Models.PredictionParameter>> Build(Models.PredictionParameter parameter)
        {
            var label = parameter.Label;
            var inputSchema = parameter.InputSchema;

            var featureColumns = inputSchema
                .Where(x => parameter.SelectedColumns.Contains(x.Name, StringComparer.InvariantCultureIgnoreCase))
                .Where(x => !x.Name.Equals(label, StringComparison.InvariantCultureIgnoreCase))                
                .Select(x => (x.Name, x.Type.ToRawType()));

            if (featureColumns is null || !featureColumns.Any())
            {
                return Result.Error("Feauture columns is empty");
            }

            parameter.FeatureColumns = featureColumns;

            return Result.Success(parameter);
        }
    }
}
