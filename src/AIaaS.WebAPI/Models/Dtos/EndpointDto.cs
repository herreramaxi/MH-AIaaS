using AIaaS.WebAPI.Models.enums;

namespace AIaaS.WebAPI.Models.Dtos
{
    public class EndpointDto : AuditableEntityDto
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public int? ModelId { get; set; }
        public int WorkflowId { get; set; }
        public string? WorkflowName { get; set; }
        public bool IsEnabled { get; set; }
        public AuthenticationType? AuthenticationType { get; set; }
        public string? ApiKey { get; set; }
    }
}
