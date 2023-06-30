using AIaaS.WebAPI.Models;
using AIaaS.WebAPI.Models.enums;
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
