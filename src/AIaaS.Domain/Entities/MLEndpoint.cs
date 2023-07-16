using AIaaS.Domain.Common;
using AIaaS.Domain.Entities.enums;
using AIaaS.Domain.Interfaces;

namespace AIaaS.Domain.Entities
{
    public class MLEndpoint : AuditableEntity, IAggregateRoot
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public AuthenticationType? AuthenticationType { get; set; }
        public MLModel? MLModel { get; set; }
        public int MLModelId { get; set; }
        public bool IsEnabled { get; set; }
        public string? ApiKey { get; set; }       
    }
}
