namespace AIaaS.Application.Common.Models
{
    public class FileStorageDto : AuditableEntityDto
    {
        public int Id { get; set; }
        public int DatasetId { get; set; }
        public DatasetDto Dataset { get; set; } = null!;
        public string FileName { get; set; }
        public long Size { get; set; }
        public string S3Key { get; set; }
        public Stream? FileStream { get; internal set; }
    }
}
