using AIaaS.Application.Common.Models;
using AIaaS.Application.Common.Models.CustomAttributes;
using AIaaS.Domain.Entities.enums;
using AIaaS.WebAPI.Interfaces;
using Ardalis.Result;
using System.Reflection;

namespace AIaaS.Application.Features.Predictions.Queries.Common.Builders
{
    public abstract class PredictServiceParameterBuilderAbstract : IPredictServiceParameterBuilder
    {
        public int Order { get; }
        public PredictServiceBuilderType BuilderType { get; }

        public PredictServiceParameterBuilderAbstract()
        {
            var builderCustomAttribute = GetType().GetCustomAttribute<PredictServiceParameterBuilderAttribute>();

            Order = builderCustomAttribute?.Order ?? int.MaxValue;
            BuilderType = builderCustomAttribute?.BuilderType ?? PredictServiceBuilderType.NA;
        }

        public abstract Task<Result<PredictionParameter>> Build(PredictionParameter parameter);
    }
}
