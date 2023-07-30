using AIaaS.Application.Common.Models.Dtos;
using System.Text.Json;

namespace AIaaS.Application.Common.Models
{
    public class WorkflowDto : AuditableEntityDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public bool? IsPublished { get; set; }
        public string? Root { get; set; }
        public bool? IsModelGenerated { get; set; }

        public WorkflowGraphDto? GetDeserializedWorkflowGraph()
        {
            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var workflowGraphDto = JsonSerializer.Deserialize<WorkflowGraphDto>(this.Root ?? "", jsonOptions);

            return workflowGraphDto;
        }

        public void UpdateSerializedRootFromGraph(WorkflowGraphDto workflowGraphDto)
        {
            this.Root = JsonSerializer.Serialize(workflowGraphDto, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }

        public void UpdateSerializedRoot(string root)
        {
            this.Root = root;
        }
    }
}
