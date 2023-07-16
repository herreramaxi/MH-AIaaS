using AIaaS.Domain.Entities;
using AIaaS.Domain.Entities.enums;
using Ardalis.Result;
using Azure.Core;
using CleanArchitecture.Application.Common.Interfaces;

namespace AIaaS.WebAPI.Infrastructure
{
    public interface ICustomAuthService
    {
        Task<Result<bool>> IsAuthenticatedAsync(HttpRequest request);
    }

    public class CustomAuthService : ICustomAuthService
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly IJwtValidationService _jwtValidationService;

        public CustomAuthService(IApplicationDbContext dbContext, IJwtValidationService jwtValidationService)
        {
            _dbContext = dbContext;
            _jwtValidationService = jwtValidationService;
        }

        public async Task<Result<bool>> IsAuthenticatedAsync(HttpRequest request)
        {
            var routeValue = request.RouteValues.FirstOrDefault(x => x.Key.Equals("endpointId", StringComparison.InvariantCultureIgnoreCase)).Value?.ToString();

            if (!int.TryParse(routeValue, out var endpointId))
            {
                return Result.Unauthorized();
            }

            var endpoint = await _dbContext.Endpoints.FindAsync(endpointId);

            if (endpoint is null)
            {
                return Result.Unauthorized();
            }

            if (!request.Method.Equals("POST", StringComparison.InvariantCultureIgnoreCase))
            {
                return Result.Unauthorized();
            }

            var result = await IsAuthenticatedAsync(request, endpoint);

            return result;
        }

        private async Task<Result<bool>> IsAuthenticatedAsync(HttpRequest request, MLEndpoint endpoint)
        {
            if (endpoint.AuthenticationType != AuthenticationType.TokenBased &&
                endpoint.AuthenticationType != AuthenticationType.JWT)
            {
                return Result.Success(true);
            }

            if (!request.Headers.TryGetValue("Authorization", out var authHeaderValues))
            {
                return Result.Unauthorized();
            }

            var authHeaderValue = authHeaderValues.FirstOrDefault();
            if (string.IsNullOrEmpty(authHeaderValue) || !authHeaderValue.StartsWith("Bearer "))
            {
                return Result.Error("No bearer token provided");
            }

            var token = authHeaderValue.Substring("Bearer ".Length);

            if (endpoint.AuthenticationType == AuthenticationType.TokenBased &&
                !token.Equals(endpoint.ApiKey))
            {
                return Result.Unauthorized();
            }

            if (endpoint.AuthenticationType == AuthenticationType.JWT)
            {
                var tokenValidationResult = await _jwtValidationService.ValidateToken(token);

                if (!tokenValidationResult.IsSuccess || !tokenValidationResult.Value.IsInRole("AIaaS-consumer"))
                    return Result.Unauthorized();

                Result.Success(true);
            }

            return Result.Success(true);
        }
    }
}