namespace AIaaS.Application.Common.Models
{
    public class CreateDatasetParameter
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public string Delimiter { get; set; }
        public bool? MissingRealsAsNaNs { get; set; }
        public IList<ColumnSettingDto>? ColumnSettings { get; set; }     
    }
}
