namespace AIaaS.Application.Common.Models
{
    public class DatasetDto : AuditableEntityDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string Delimiter { get; set; }
        public bool? MissingRealsAsNaNs { get; set; }
        public IList<ColumnSettingDto>? ColumnSettings { get; set; }     
        public long? Size { get; set; }
        public string? FileName { get; set; }
        public string? FileUrl{ get; set; }
        public string? DataViewFileName { get; set; }
        public long? DataViewFileSize { get; set; }
        public string? DataViewFileUrl { get; set; }
    }
}
