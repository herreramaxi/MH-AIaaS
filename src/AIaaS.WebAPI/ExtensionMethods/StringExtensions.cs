using AIaaS.WebAPI.Models.enums;
using Microsoft.ML.Data;

namespace AIaaS.WebAPI.ExtensionMethods
{
    public static class StringExtensions
    {
        public static DataKind ToDataKind(this string typeAsString)
        {
            Enum.TryParse<ColumnDataTypeEnum>(typeAsString, out var columnDataTypeEnum);

            switch (columnDataTypeEnum)
            {
                case ColumnDataTypeEnum.String: return DataKind.String;
                case ColumnDataTypeEnum.Int: return DataKind.Int32;
                case ColumnDataTypeEnum.Datetime: return DataKind.DateTime;
                case ColumnDataTypeEnum.Decimal: return DataKind.Double;
                default: return DataKind.String;
            }
        }

        public static Type ToDataType(this string typeAsString)
        {
            Enum.TryParse<ColumnDataTypeEnum>(typeAsString, out var columnDataTypeEnum);

            switch (columnDataTypeEnum)
            {
                case ColumnDataTypeEnum.String: return typeof(string);
                case ColumnDataTypeEnum.Int: return typeof(int);
                case ColumnDataTypeEnum.Datetime: return typeof(DateTime);
                case ColumnDataTypeEnum.Decimal: return typeof(float);
                default: return typeof(string);
            }
        }
    }
}
