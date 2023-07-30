namespace AIaaS.Application.Common.Models.Dtos
{
    public class OperatorDto
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public OperatorDataDto Data { get; set; } = new OperatorDataDto();
        public int Order { get;  set; }
    }
}
