namespace AIaaS.Application.Common.Models.Dtos
{
    public class ColumnTransformationDto
    {
        public string  ColumnNameIn { get; set; }
        public string ColumnNameOut { get; set; }
        public string DataType { get; set; }
        public string CategoricalType { get; set; }
    }
}
