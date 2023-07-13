using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AIaaS.WebAPI.Services
{
    public interface IJwtValidationService
    {
        Task<ClaimsPrincipal> ValidateToken(string token);
    }

    public class JwtValidationService : IJwtValidationService
    {
        private readonly string _issuer;
        private readonly string _audience;
        private string _jwksEndpoint;
        private readonly string _secret;

        public JwtValidationService(string issuer, string audience, string jwksEndpoint)
        {
            _issuer = issuer;
            _audience = audience;
            _jwksEndpoint = jwksEndpoint;
        }

        public async  Task<ClaimsPrincipal> ValidateToken(string token)
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
                // Validate the token
                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
                return principal;
            }
            catch (Exception ex)
            {
                // Token validation failed
                // Handle or log the exception
                throw new Exception("Token validation failed.", ex);
            }
        }
    }
}

