using AIaaS.Application.Common.Models;
using AIaaS.Application.Common.Models.CustomAttributes;
using AIaaS.Application.Specifications.Endpoints;
using AIaaS.Domain.Entities;
using AIaaS.Domain.Entities.enums;
using AIaaS.Domain.Interfaces;
using Ardalis.Result;

namespace AIaaS.Application.Features.Predictions.Queries.Common.Builders
{
    [PredictServiceParameterBuilder(PredictServiceBuilderType.Endpoint, 2)]
    public class EndpointBuilder : PredictServiceParameterBuilderAbstract
    {
        private readonly IReadRepository<MLEndpoint> _repository;

        public EndpointBuilder(IReadRepository<MLEndpoint> repository)
        {
            _repository = repository;
        }

        public override async Task<Result<PredictionParameter>> Build(PredictionParameter parameter)
        {
            var endpoint = await _repository.FirstOrDefaultAsync(new GetEndpointByIdWithMLModelAndWorkflowSpec(parameter.EndpointId));
         
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
