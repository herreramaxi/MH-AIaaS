using AIaaS.Application.Common.Models;
using AIaaS.Domain.Entities.enums;
using Ardalis.Result;

namespace AIaaS.WebAPI.Interfaces
{
    public interface IPredictServiceParameterBuilder
    {
        int Order { get; }
        PredictServiceBuilderType BuilderType { get; }
        Task<Result<PredictionParameter>> Build(PredictionParameter parameter);
    }
}
