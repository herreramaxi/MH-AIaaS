using System.ComponentModel.DataAnnotations.Schema;

namespace AIaaS.WebAPI.Models.Dtos
{
    public class MlModelDto : AuditableEntityDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsPublished { get; set; }
        [NotMapped]
        public Workflow? Workflow { get; internal set; }
        public int WorkflowId { get; internal set; }
        public long Size { get; internal set; }
        public string FileName { get; internal set; }
    }
}
