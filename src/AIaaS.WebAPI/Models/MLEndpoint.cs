using AIaaS.WebAPI.Models.enums;

namespace AIaaS.WebAPI.Models
{
    public class MLEndpoint : AuditableEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public AuthenticationType? AuthenticationType { get; set; }
        public MLModel? MLModel { get; set; }
        public int MLModelId { get; set; }
        public bool IsEnabled { get; set; }
        public string? ApiKey { get; set; }

        public static implicit operator int(MLEndpoint? v)
        {
            throw new NotImplementedException();
        }
    }
}
