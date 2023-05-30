namespace AIaaS.WebAPI.Models.Dtos
{
    public class WorkflowNodeDto: OperatorDto
    {
        public string Id{ get; set; }
        public IList<WorkflowNodeDto>? Children { get; set; }
    }
}
