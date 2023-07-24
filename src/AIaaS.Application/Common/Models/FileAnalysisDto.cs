namespace AIaaS.Application.Common.Models.Dtos
{
    public class FileAnalysisDto
    {
        public string[] Header { get; set; }
        public string Delimiter{ get; set; }
        public List<ColumnSettingDto> ColumnsSettings { get; set; } = new List<ColumnSettingDto>();
        public string[][] Rows { get; set; }
        public int? TotalRows { get; set; }
        public int? TotalColumns { get; set; }
        public int? PreviewRows { get; internal set; }
    }
}
