namespace AIaaS.WebAPI.Models.Dtos
{
    public class OperatorDto
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public OperatorDataDto? Data { get; set; }
        public int Order { get;  set; }
    }
}
