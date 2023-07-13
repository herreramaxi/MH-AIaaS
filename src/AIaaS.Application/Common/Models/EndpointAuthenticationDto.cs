using AIaaS.Domain.Entities.enums;

namespace AIaaS.Application.Common.Models.Dtos
{
    public record EndpointAuthenticationDto(int Id, AuthenticationType? AuthenticationType, string? ApiKey);
}
