using AIaaS.Application.Common.ExtensionMethods;
using AIaaS.Application.Common.Models;
using AIaaS.Application.Interfaces;
using AIaaS.Domain.Entities;
using AIaaS.Domain.Interfaces;
using AIaaS.WebAPI.ExtensionMethods;
using Ardalis.Result;
using Microsoft.Extensions.Logging;
using Microsoft.ML;

namespace AIaaS.Application.Services
{
    public class DataViewService : IDataViewService
    {
        private const int TOP_ROWS_PREVIEW = 500;
        private readonly IS3Service _s3Service;
        private readonly ILogger<DataViewService> _logger;

        public DataViewService(IS3Service s3Service, ILogger<DataViewService> logger)
        {
            _s3Service = s3Service;
            _logger = logger;
        }

        public async Task<Result<DataViewFilePreviewDto>> GetPreviewAsync(IDataViewFile dataViewFile)
        {
            try
            {
                var dataViewResult = await this.GetDataViewAsync(dataViewFile);
                if (!dataViewResult.IsSuccess)
                {
                    return Result.Error(dataViewResult.Errors.FirstOrDefault() ?? "Error when trying to download dataView file from S3");
                }

                var dataview = dataViewResult.Value;
                var header = dataview.Schema.Select(x => x.Name);
                var totalColumns = dataview.Schema.Count;
                var totalRows = (int?)dataview.GetRowCount();
                var preview = dataview.Preview(maxRows: TOP_ROWS_PREVIEW);
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
                    TotalColumns = totalColumns,
                    TopRows = TOP_ROWS_PREVIEW
                };

                return dataPreview;
            }
            catch (Exception exception)
            {
                var errorMessage = "Error when trying to generate preview for dataview";
                _logger.LogError(exception, errorMessage);
                return Result.Error(errorMessage);
            }
        }

        public async Task<Result<IDataView>> GetDataViewAsync(IDataViewFile dataViewFile)
        {
            if (dataViewFile is null) return Result.Error("DataViewFile is null");

            var result = await _s3Service.DownloadFileAsync(dataViewFile.S3Key);
            if (!result.IsSuccess)
            {
                return Result.Error(result.Errors.FirstOrDefault());
            }

            var memoryStream = result.Value.ToMemoryStream();
            var mss = new MultiStreamSourceFile(memoryStream);
            var mlContext = new MLContext();
            var dataview = mlContext.Data.LoadFromBinary(mss);

            return Result.Success(dataview);
        }     
    }
}
