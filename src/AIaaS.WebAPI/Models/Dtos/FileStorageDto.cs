namespace AIaaS.WebAPI.Models
{
    public class FileStorageDto: AuditableEntityDto
    {
        public int Id { get; set; }
        public int DatasetId { get; set; }
        public DatasetDto Dataset { get; set; } = null!;
        public string FileName { get; set; }
        public long Size { get; set; }
        public byte[] Data { get; set; } 
    }
}
