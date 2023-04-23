namespace AIaaS.WebAPI.Models.Dtos
{
    public class FileAnalysisDto
    {
        public string[] Header { get; set; }
        public List<ColumnSettingDto> ColumnsSettings { get; set; } = new List<ColumnSettingDto>();
        public string[][] Data { get; set; }
    }
}
