namespace AIaaS.WebAPI.Models.Dtos
{
    public class WorkflowGraphDto: OperatorDto
    {
        public string Id { get; set; }
        public IList<WorkflowGraphDto> Children { get; set; } = new List<WorkflowGraphDto>();
    }
}
