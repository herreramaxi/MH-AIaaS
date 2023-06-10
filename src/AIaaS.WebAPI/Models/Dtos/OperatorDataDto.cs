namespace AIaaS.WebAPI.Models.Dtos
{
    public class OperatorDataDto
    {
        public string Name { get; set; }
        public IList<OperatorConfigurationDto>? Config { get; set; }
        public bool IsFailed { get; set; }
        public string? ValidationMessage { get; set; }
        public IDictionary<string, object>? Parameters { get; set; }
        public IList<string>? DatasetColumns { get; set; }
    }
}
