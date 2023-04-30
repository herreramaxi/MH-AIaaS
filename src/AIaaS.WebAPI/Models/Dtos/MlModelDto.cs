using System.ComponentModel.DataAnnotations.Schema;

namespace AIaaS.WebAPI.Models.Dtos
{
    public class MlModelDto : AuditableEntityDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        [NotMapped]
        public Workflow? Workflow { get; internal set; }
    }
}
