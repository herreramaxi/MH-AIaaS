namespace AIaaS.Application.Common.Models
{
    public class DataViewFilePreviewDto
    {
        public IEnumerable<string> Header { get; set; }
        public IEnumerable<string[]> Rows { get; set; }
        public int TotalRows { get; set; }
        public int TotalColumns { get;  set; }
    }
}
