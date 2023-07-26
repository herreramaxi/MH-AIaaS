using AIaaS.Application.Common.Models;
using AIaaS.Domain.Entities;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace AIaaS.WebAPI.ExtensionMethods
{
    public static class MLExtensionMethods
    {
        public static Type ToRawType(this DataViewType dataViewType)
        {
            return dataViewType is TextDataViewType ?
                typeof(string) :
                dataViewType.RawType;
        }

        public static IEstimator<ITransformer> AppendEstimator(this IEstimator<ITransformer> estimatorChain, IEstimator<ITransformer> estimator)
        {
            return estimatorChain is not null ?
                 estimatorChain.Append(estimator) :
                 estimator;
        }

        public static DataViewFilePreviewDto? GetPreview(this byte[] dataviewData) {
            if (dataviewData is null) return null;

            using var memStream = new MemoryStream(dataviewData);
            var mss = new MultiStreamSourceFile(memStream);
            var mlContext = new MLContext();
            var dataview = mlContext.Data.LoadFromBinary(mss);
            var header = dataview.Schema.Select(x => x.Name);
            var totalColumns = dataview.Schema.Count;
            var totalRows = (int?)dataview.GetRowCount()??100;
            var preview = dataview.Preview(maxRows: totalRows);
            var records = new List<string[]>();

            foreach (var row in preview.RowView)
            {
                var record = row.Values
                    .Select(x => x.Value?.ToString() ?? "")
                    .ToArray();

                records.Add(record);
            }

            var dataPreview = new DataViewFilePreviewDto
            {
                Header = header,
                Rows = records,
                TotalRows = totalRows,
                TotalColumns = totalColumns
            };

            return dataPreview;
        }              
    }
}
