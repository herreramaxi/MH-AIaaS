using AIaaS.Application.Common.Models;
using AIaaS.Application.Common.Models.CustomAttributes;
using AIaaS.Domain.Entities.enums;
using AIaaS.WebAPI.ExtensionMethods;
using Ardalis.Result;

namespace AIaaS.Application.Features.Predictions.Queries.Common.Builders
{
    [PredictServiceParameterBuilder(PredictServiceBuilderType.Runtime, 5)]
    public class RuntimeParameterBuilder : PredictServiceParameterBuilderAbstract
    {
        public override async Task<Result<PredictionParameter>> Build(PredictionParameter parameter)
        {
            var predictionProperties = parameter.Task.Equals("Regression", StringComparison.InvariantCultureIgnoreCase) ?
             new[] { (parameter.Label, parameter.LabelColumn.Type.ToRawType(), "Score") } :
              new[] { (parameter.Label, parameter.LabelColumn.Type.ToRawType(), "PredictedLabel"), ("Score", typeof(float), "Score") };

            var runtimeTypeInput = ClassFactory.CreateType(parameter.InputSchema);
            var runtimeTypeOutput = ClassFactory.CreateType(predictionProperties);

            if (runtimeTypeInput is null)
            {
                return Result.Error("Error when trying to create runtime type input");
            }

            if (runtimeTypeOutput is null)
            {
                return Result.Error("Error when trying to create runtime type output");
            }

            parameter.RuntimeTypeInput = runtimeTypeInput;
            parameter.RuntimeTypeOutput = runtimeTypeOutput;

            return Result.Success(parameter);
        }
    }
}
