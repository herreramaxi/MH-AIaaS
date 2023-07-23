using AIaaS.Application.Common.Models;
using AIaaS.Domain.Entities.enums;
using AIaaS.WebAPI.Interfaces;
using Ardalis.Result;

namespace AIaaS.Application.Features.Predictions.Queries.Common
{
    public class PredictionBuilderDirector : IPredictionBuilderDirector
    {
        private readonly IEnumerable<IPredictServiceParameterBuilder> _builders;

        public PredictionBuilderDirector(IEnumerable<IPredictServiceParameterBuilder> builders)
        {
            _builders = builders;
        }

        public async Task<Result<PredictionParameter>> BuildInputSampleParameter(PredictionParameter parameter)
        {
            var builders = GetBuilders(new PredictServiceBuilderType[] {
                PredictServiceBuilderType.Endpoint,
                PredictServiceBuilderType.Model,
                PredictServiceBuilderType.Workflow,
                PredictServiceBuilderType.Runtime,
                PredictServiceBuilderType.FeatureColumns
            });
            var result = await ExecuteBuilders(parameter, builders);

            return result;
        }

        public async Task<Result<PredictionParameter>> BuildPredictionParameter(PredictionParameter parameter)
        {
            var builders = GetBuilders();
            var result = await ExecuteBuilders(parameter, builders);

            return result;
        }

        public IEnumerable<IPredictServiceParameterBuilder> GetBuilders(params PredictServiceBuilderType[] builderType)
        {
            return _builders.Where(x => !builderType.Any() || builderType.Contains(x.BuilderType));
        }

        private async Task<Result<PredictionParameter>> ExecuteBuilders(PredictionParameter parameter, IEnumerable<IPredictServiceParameterBuilder> builders)
        {
            if (!builders.Any())
            {
                return Result.Error("No builders found to be executed");
            }

            foreach (var builder in builders)
            {
                var result = await builder.Build(parameter);

                if (!result.IsSuccess)
                {
                    return result;
                }
            }

            return Result.Success(parameter);
        }
    }
}
