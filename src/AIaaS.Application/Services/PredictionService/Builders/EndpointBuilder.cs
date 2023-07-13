using AIaaS.Application.Common.Models;
using AIaaS.Application.Common.Models.CustomAttributes;
using AIaaS.Domain.Entities.enums;
using Ardalis.Result;
using CleanArchitecture.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AIaaS.WebAPI.Services.PredictionService.Builders
{
    [PredictServiceParameterBuilder(PredictServiceBuilderType.Endpoint, 2)]
    public class EndpointBuilder : PredictServiceParameterBuilderAbstract
    {
        private readonly IApplicationDbContext _dbContext;

        public EndpointBuilder(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public override async Task<Result<PredictionParameter>> Build(PredictionParameter parameter)
        {
            var endpoint = await _dbContext.Endpoints
                .Include(e => e.MLModel)
                .ThenInclude(m => m.Workflow)
                .FirstOrDefaultAsync(e => e.Id == parameter.EndpointId);

            if (endpoint is null)
            {
                return Result.NotFound("Endpoint not found");
            }

            if (!endpoint.IsEnabled)
            {
                return Result.Error("Endpoint is disabled");
            }

            parameter.Endpoint = endpoint;

            return Result.Success(parameter);
        }
    }
}
