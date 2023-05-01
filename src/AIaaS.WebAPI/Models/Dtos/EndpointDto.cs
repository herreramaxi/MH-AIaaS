namespace AIaaS.WebAPI.Models.Dtos
{
    public class EndpointDto: AuditableEntityDto
    {
        public string Name { get; set; }
        public string ModelName{ get; set; }
        public int ModelId{ get; set; }
        public string WorkflowName{ get; set; }
        public int WorkflowId { get; set; }
        public bool IsEnabled { get; set; }
        public string AuthenticationMethod { get; set; } = "API Key";
        public string ApiKey { get; set; } = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()));
        public int Id { get; set; }
    }
}
