using AIaaS.Application.Common.Models;
using AIaaS.Domain.Entities.enums;
using Ardalis.Result;

namespace AIaaS.WebAPI.Interfaces
{
    public interface IPredictionBuilderDirector
    {
        IEnumerable<IPredictServiceParameterBuilder> GetBuilders(params PredictServiceBuilderType[] builderType);
        Task<Result<PredictionParameter>> BuildPredictionParameter(PredictionParameter parameter);
        Task<Result<PredictionParameter>> BuildInputSampleParameter(PredictionParameter parameter);
    }
}
