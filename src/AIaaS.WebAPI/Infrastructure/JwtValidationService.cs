using Ardalis.Result;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AIaaS.WebAPI.Infrastructure
{
    public interface IJwtValidationService
    {
        Task<Result<ClaimsPrincipal>> ValidateToken(string token);
    }

    public class JwtValidationService : IJwtValidationService
    {
        private readonly string _issuer;
        private readonly string _audience;
        private string _jwksEndpoint;
        private readonly ILogger<IJwtValidationService> _logger;
        private readonly string _secret;

        public JwtValidationService(string issuer, string audience, string jwksEndpoint, ILogger<IJwtValidationService> logger)
        {
            _issuer = issuer;
            _audience = audience;
            _jwksEndpoint = jwksEndpoint;
            _logger = logger;
        }

        public async Task<Result<ClaimsPrincipal>> ValidateToken(string token)
        {
            var httpClient = new HttpClient();
            //TODO: improve this, add inmemory cache
            var jwksResponse = await httpClient.GetStringAsync(_jwksEndpoint);
            var jwks = new JsonWebKeySet(jwksResponse);

            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _issuer,
                ValidAudience = _audience,
                //IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_secret))
                IssuerSigningKey = jwks.GetSigningKeys()?.FirstOrDefault()
            };

            try
            {
                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
                return Result.Success(principal);
            }
            catch (Exception ex)
            {
                var errorMessage = $"Token validation failed: {ex.Message}";
                _logger.LogError(ex, errorMessage);

                return Result.Error(errorMessage);
            }
        }
    }
}

