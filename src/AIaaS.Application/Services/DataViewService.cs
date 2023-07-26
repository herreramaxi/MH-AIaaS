using AIaaS.Application.Common.ExtensionMethods;
using AIaaS.Application.Common.Models;
using AIaaS.Application.Interfaces;
using AIaaS.Domain.Entities;
using AIaaS.Domain.Interfaces;
using Ardalis.Result;
using Microsoft.ML;

namespace AIaaS.Application.Services
{
    public class DataViewService : IDataViewService
    {
        private readonly IS3Service _s3Service;

        public DataViewService(IS3Service s3Service)
        {
            _s3Service = s3Service;
        }

        public async Task<Result<DataViewFilePreviewDto>> GetPreviewAsync(DataViewFile dataViewFile)
        {
            var dataViewResult = await this.GetDataViewAsync(dataViewFile);
            if (!dataViewResult.IsSuccess)
            {
                return Result.Error(dataViewResult.Errors.FirstOrDefault() ?? "Error when trying to download dataView file from S3");
            }

            var dataview = dataViewResult.Value;
            var header = dataview.Schema.Select(x => x.Name);
            var totalColumns = dataview.Schema.Count;
            var totalRows = (int?)dataview.GetRowCount() ?? 100;
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

        public async Task<Result<IDataView>> GetDataViewAsync(DataViewFile dataViewFile)
        {
            if (dataViewFile is null) return Result.Error("DataViewFile is null");

            var fileStream = await _s3Service.DownloadFileAsync(dataViewFile.S3Key);
            if (fileStream is null)
            {
                return Result.Error("Not able to download dataview file from S3");
            }

            var memoryStream = fileStream.ToMemoryStream();
            var mss = new MultiStreamSourceFile(memoryStream);
            var mlContext = new MLContext();
            var dataview = mlContext.Data.LoadFromBinary(mss);
            
            return Result.Success(dataview);
        }
    }
}
