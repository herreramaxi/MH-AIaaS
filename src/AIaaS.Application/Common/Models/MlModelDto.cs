using AIaaS.Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace AIaaS.Application.Common.Models.Dtos
{
    public class MlModelDto : AuditableEntityDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsPublished { get; set; }
        [NotMapped]
        public Workflow? Workflow { get; set; }
        public int WorkflowId { get; set; }
        public long Size { get; set; }
        public string FileName { get; set; }
    }
}
