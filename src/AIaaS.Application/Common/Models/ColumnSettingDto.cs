using AIaaS.Application.Common.Models;

namespace AIaaS.Application.Common.Models
{
    public class ColumnSettingDto: AuditableEntityDto
    {
        public int Id { get; set; }
        public int DatasetId { get; set; }
        public string ColumnName { get; set; }
        public bool Include { get; set; }
        public string Type { get; set; }
    }

}
