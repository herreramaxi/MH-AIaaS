namespace AIaaS.Application.Common.Models
{
    public class PredictionInputDto
    {
        //public int EndpointId { get; set; }
        public string[]? Columns { get; set; }
        public object[]? Data { get; set; }
    }
}
