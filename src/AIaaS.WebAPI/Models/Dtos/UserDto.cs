namespace AIaaS.WebAPI.Models
{
    public class UserDto: AuditableEntityDto
    {
        public int Id{ get; set; }
        public string Email { get; set; }
        //public string Role { get; set; }
        public List<DatasetDto> Datasets { get; set; } = new List<DatasetDto>();
    }
}
