using AIaaS.Application.Common.ExtensionMethods;
using AIaaS.Application.Common.Models;
using AIaaS.Application.Common.Models.Dtos;
using AIaaS.WebAPI.ExtensionMethods;
using Ardalis.Result;
using CsvHelper;
using CsvHelper.Configuration;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.ML;
using Microsoft.ML.AutoML;
using Microsoft.ML.Data;
using System.Data;
using System.Globalization;

namespace AIaaS.Application.Features.Datasets.Queries.GenerateFilePreview
{
    public class GenerateFileAnalysisRequestHandler : IRequestHandler<GenerateFileAnalysisRequest, Result<FileAnalysisDto>>
    {
        private const int MAX_ROWS = 500;
        private readonly ILogger<GenerateFileAnalysisRequestHandler> _logger;

        public GenerateFileAnalysisRequestHandler(ILogger<GenerateFileAnalysisRequestHandler> logger)
        {
            _logger = logger;
        }

        public async Task<Result<FileAnalysisDto>> Handle(GenerateFileAnalysisRequest request, CancellationToken cancellationToken)
        {
            var filePath = default(string);
            try
            {
                var fileAnalysis = new FileAnalysisDto();
                var file = request.GenerateFileAnalysisParameter.File;
                var delimiter = request.GenerateFileAnalysisParameter.Delimiter;
                var missingRealsAsNaNs = request.GenerateFileAnalysisParameter.MissingRealsAsNaNs ?? false;

                fileAnalysis.Delimiter = !string.IsNullOrEmpty(delimiter) ?
                    delimiter.ToStringDelimiter() :
                    file.FileName.Contains(".tsv", StringComparison.InvariantCultureIgnoreCase) ? "\t" : ",";

                filePath = await file.SaveTempFile();
                fileAnalysis.Header = GetHeader(file, fileAnalysis);

                if (!fileAnalysis.Header.Any())
                {
                    return Result.Error("Header is empty");
                }

                var labelColumn = fileAnalysis.Header
                    .Where(x => x.Equals("Label", StringComparison.InvariantCultureIgnoreCase))
                    .FirstOrDefault() ?? fileAnalysis.Header.Last();

                var mlContext = new MLContext();
                var separatorChar = fileAnalysis.Delimiter.ToCharDelimiter();
                var columnInference = mlContext.Auto().InferColumns(filePath, labelColumnName: labelColumn, groupColumns: false, separatorChar: separatorChar);
                columnInference.TextLoaderOptions.MissingRealsAsNaNs = missingRealsAsNaNs;

                fileAnalysis.ColumnsSettings = columnInference.TextLoaderOptions.Columns.Select(x =>
                      new ColumnSettingDto
                      {
                          ColumnName = x.Name,
                          Include = true,
                          Type = x.DataKind.ToString(),
                      }
                     ).ToList();

                TextLoader loader = mlContext.Data.CreateTextLoader(columnInference.TextLoaderOptions);
                IDataView data = loader.Load(filePath);
                fileAnalysis.TotalColumns = data.Schema.Count;
                fileAnalysis.TotalRows = (int?)data.GetRowCount();           
                var preview = data.Preview(maxRows: MAX_ROWS);

                var records = new List<string[]>();

                foreach (var row in preview.RowView)
                {
                    var record = new string[row.Values.Length];
                    var values = row.Values.Select(x => x.Value).ToArray();
                    var ColumnCollection = row.Values;

                    for (int i = 0; i < row.Values.Length; i++)
                    {
                        record[i] = values[i]?.ToString() ?? "";
                    }

                    records.Add(record);
                }
                                
                fileAnalysis.Rows = records.ToArray();
                fileAnalysis.Delimiter = fileAnalysis.Delimiter.Replace("\t", "\\t");
                fileAnalysis.PreviewRows = records.Count;

                return Result.Success(fileAnalysis);
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error when generating file preview: {ex.Message}";
                _logger.LogError(ex, errorMessage);

                return Result.Error(errorMessage);
            }
            finally
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }

        private string[] GetHeader(IFormFile file, FileAnalysisDto fileAnalysis)
        {
            using var reader = file.OpenReadStream();
            using var tr = new StreamReader(reader);
            var csvConfiguration = new CsvConfiguration(CultureInfo.CurrentCulture);
            csvConfiguration.Delimiter = fileAnalysis.Delimiter;
            csvConfiguration.HasHeaderRecord = true;
            using var csv = new CsvReader(tr, csvConfiguration);
            csv.Read();
            csv.ReadHeader();
            return csv.Context.Reader.HeaderRecord ?? Array.Empty<string>();
        }
    }
}
