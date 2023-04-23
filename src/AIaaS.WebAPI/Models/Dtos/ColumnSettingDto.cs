namespace AIaaS.WebAPI.Models
{
    public class ColumnSettingDto: AuditableEntityDto
    {
        public int Id { get; set; }
        public int DatasetId { get; set; }
        public string ColumnName { get; set; }
        public bool Include { get; set; }
        //"text" | "numeric" | "boolean" | "date"
        public string Type { get; set; }
    }

}
