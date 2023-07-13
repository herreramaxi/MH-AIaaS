using AIaaS.Application.Common.Models;
using AIaaS.Application.Common.Models.CustomAttributes;
using AIaaS.Domain.Entities.enums;
using AIaaS.WebAPI.Interfaces;
using Ardalis.Result;
using System.Reflection;

namespace AIaaS.WebAPI.Services.PredictionService.Builders
{
    public abstract class PredictServiceParameterBuilderAbstract : IPredictServiceParameterBuilder
    {
        public int Order { get; }
        public PredictServiceBuilderType BuilderType { get; }

        public PredictServiceParameterBuilderAbstract()
        {
            var builderCustomAttribute = this.GetType().GetCustomAttribute<PredictServiceParameterBuilderAttribute>();

            this.Order = builderCustomAttribute?.Order??int.MaxValue;
            this.BuilderType = builderCustomAttribute?.BuilderType ??PredictServiceBuilderType.NA;
        }

        public abstract Task<Result<PredictionParameter>> Build(PredictionParameter parameter);
    }
}
