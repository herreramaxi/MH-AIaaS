using AIaaS.WebAPI.Data;
using Ardalis.Result;

namespace AIaaS.WebAPI.Services
{
    public interface ICustomAuthService
    {
        Task<Result<bool>> IsAuthenticatedAsync(HttpRequest request);
    }

    public class CustomAuthService : ICustomAuthService
    {
        private readonly EfContext _dbContext;
        private readonly IMyCustomService _myCustomService;

        public CustomAuthService(EfContext dbContext, IMyCustomService myCustomService)
        {
            _dbContext = dbContext;
            _myCustomService = myCustomService;
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

        private async Task<Result<bool>> IsAuthenticatedAsync(HttpRequest request, Models.MLEndpoint endpoint)
        {
            if (endpoint.AuthenticationType != Models.enums.AuthenticationType.TokenBased &&
                endpoint.AuthenticationType != Models.enums.AuthenticationType.JWT)
            {
                return Result.Success(true);
            }

            var bearerHeader = request.Headers.FirstOrDefault(x => x.Key.Equals("Bearer", StringComparison.InvariantCultureIgnoreCase));

            if (string.IsNullOrEmpty(bearerHeader.Value))
            {
                return Result.Unauthorized();
            }

            if (endpoint.AuthenticationType == Models.enums.AuthenticationType.TokenBased &&
                !bearerHeader.Value.Equals(endpoint.ApiKey))
            {
                return Result.Unauthorized();
            }

            if (endpoint.AuthenticationType == Models.enums.AuthenticationType.JWT)
            {
                var isAuthenticated = await _myCustomService.IsAuthenticated(bearerHeader.Value);

                return isAuthenticated ? Result.Success(true) : Result.Unauthorized();
            }

            return Result.Success(true);
        }
    }
}