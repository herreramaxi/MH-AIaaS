using AIaaS.WebAPI.Models;
using AIaaS.WebAPI.Models.enums;
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
