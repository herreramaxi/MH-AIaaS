namespace AIaaS.WebAPI.Models.Dtos
{
    public class OperatorDataDto
    {
        public string Name{ get; set; }
        public IList<OperatorConfigurationDto> Config { get; set; }
        public bool IsValid { get; set; }
        public string ValidationMessage { get; set; }
        public IDictionary<string,object> Parameters { get; set; }
    }
}
