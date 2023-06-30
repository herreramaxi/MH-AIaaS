using AIaaS.WebAPI.Models.enums;

namespace AIaaS.WebAPI.Models.Dtos
{
    public record EndpointAuthenticationDto(int Id, AuthenticationType? AuthenticationType, string? ApiKey);
}
