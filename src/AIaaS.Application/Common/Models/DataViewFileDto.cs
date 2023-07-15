namespace AIaaS.Application.Common.Models
{
    public class DataViewFileDto: AuditableEntityDto
    {
        public int DatasetId { get; set; }
        public long Size { get; set; }
        public byte[] Data { get; set; }
        public string? Name { get; set; }
    }
}
