using AIaaS.Domain.Entities.enums;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.ML.Data;

namespace AIaaS.WebAPI.ExtensionMethods
{
    public static class StringExtensions
    {
        public static DataKind ToDataKind(this string typeAsString)
        {
            Enum.TryParse<DataKind>(typeAsString, out var columnDataTypeEnum);

            return columnDataTypeEnum;
        }

        public static Type ToType(this DataKind dataKind)
        {
            var dataType = dataKind switch
            {
                DataKind.Boolean => typeof(bool),
                DataKind.Single => typeof(float),
                DataKind.Double => typeof(double),
                DataKind.Int16 => typeof(short),
                DataKind.UInt16 => typeof(ushort),
                DataKind.Int32 => typeof(int),
                DataKind.UInt32 => typeof(uint),
                DataKind.Int64 => typeof(long),
                DataKind.UInt64 => typeof(ulong),
                DataKind.String => typeof(string),
                DataKind.DateTime => typeof(DateTime),
                DataKind.TimeSpan => typeof(TimeSpan),
                DataKind.DateTimeOffset => typeof(DateTimeOffset),

                _ => typeof(float)
            };

            return dataType;
        }


        //public static DataKind ToDataKind(this string typeAsString)
        //{
        //    Enum.TryParse<ColumnDataTypeEnum>(typeAsString, out var columnDataTypeEnum);

        //    switch (columnDataTypeEnum)
        //    {
        //        case ColumnDataTypeEnum.String: return DataKind.String;
        //        case ColumnDataTypeEnum.Int: return DataKind.Int32;
        //        case ColumnDataTypeEnum.Datetime: return DataKind.DateTime;
        //        case ColumnDataTypeEnum.Decimal: return DataKind.Double;
        //        default: return DataKind.String;
        //    }
        //}

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

        public static char ToCharDelimiter(this string delimiter)
        {
            var delimiterAsChar = delimiter.Replace("\\t", "\t").ToCharArray().FirstOrDefault();
            delimiterAsChar = delimiterAsChar == default ? ',' : delimiterAsChar;

            return delimiterAsChar;
        }

        public static string ToStringDelimiter(this string delimiter)
        {
            return delimiter.Replace("\\t", "\t");
        }

        public static string GenerateS3Key(this string fileName)
        {
            return $"{Guid.NewGuid()}_{fileName}";
        }
    }
}
