using AIaaS.Domain.Entities;

namespace AIaaS.Domain.Common
{
    public class AuditableEntity: Entity
    {
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string? ModifiedBy { get; set; }
    }
}
