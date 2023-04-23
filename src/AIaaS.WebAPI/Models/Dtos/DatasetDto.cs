namespace AIaaS.WebAPI.Models
{
    public class DatasetDto: AuditableEntityDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public UserDto? User { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Delimiter { get; set; }
        public ICollection<ColumnSettingDto> ColumnSettings { get; set; }
        public FileStorageDto? FileStorage { get; set; }
    }
}
