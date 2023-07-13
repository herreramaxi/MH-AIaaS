using AIaaS.Application.Common.Models;
using Ardalis.Result;
using static AIaaS.WebAPI.Services.PredictionService.PredictionService;

namespace AIaaS.WebAPI.Interfaces
{
    public interface IPredictionService
    {
        Task<Result<SimplePrediction>> Predict(int endpointId, PredictionInputDto? predictionInputDto);
        Task<Result<object>> Predict(int endpointId, StreamReader streamReader, bool onlyPredictionProperties = false);
    }
}
