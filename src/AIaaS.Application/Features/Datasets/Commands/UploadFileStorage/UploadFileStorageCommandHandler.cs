using AIaaS.Application.Common.ExtensionMethods;
using AIaaS.Application.Specifications.Datasets;
using AIaaS.Domain.Entities;
using AIaaS.Domain.Interfaces;
using AIaaS.WebAPI.ExtensionMethods;
using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace AIaaS.Application.Features.Datasets.Commands.UploadDataset
{
    public class UploadFileStorageCommandHandler : IRequestHandler<UploadFileStorageCommand, Result>
    {
        private readonly IRepository<Dataset> _repository;
        private readonly ILogger<UploadFileStorageCommandHandler> _logger;

        public UploadFileStorageCommandHandler(IRepository<Dataset> repository, ILogger<UploadFileStorageCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }
        public async Task<Result> Handle(UploadFileStorageCommand request, CancellationToken cancellationToken)
        {
            var filePath = default(string);

            try
            {
                var dataset = await _repository.FirstOrDefaultAsync(new DatasetByIdWithColumnSettingsSpec(request.UploadFileStorageParameter.DatasetId));
                if (dataset is null)
                {
                    return Result.NotFound("Dataset not found");
                }

                var file = request.UploadFileStorageParameter.File;
                filePath = await file.SaveTempFile();
                using var reader = file.OpenReadStream();
                using var memStream = new MemoryStream();
                await reader.CopyToAsync(memStream);

                var fileStorage = new FileStorage()
                {
                    FileName = file.FileName,
                    Size = file.Length,
                    Data = memStream.ToArray()
                };

                dataset.FileStorage = fileStorage;

                reader.Seek(0, SeekOrigin.Begin);

                dataset.DataViewFile = CreateDataViewFile(filePath, dataset, file);

                await _repository.UpdateAsync(dataset);

                return Result.Success();
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error when uploading file: {ex.Message}";
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

        private static DataViewFile CreateDataViewFile(string? filePath, Dataset dataset, IFormFile file)
        {
            var mlContext = new MLContext();
            var columns = dataset.ColumnSettings.Select((x, index) => new TextLoader.Column(x.ColumnName, x.Type.ToDataKind(), index)).ToArray();
            var separator = dataset.Delimiter.ToCharDelimiter();
            var options = new TextLoader.Options
            {
                AllowQuoting = true,
                HasHeader = true,
                MissingRealsAsNaNs = dataset.MissingRealsAsNaNs ?? false,
                Separators = new[] { separator },
                Columns = columns
            };

            var columnNames = dataset.ColumnSettings.Select(x => x.ColumnName);
            var labelColumn = columnNames
                .Where(x => x.Equals("Label", StringComparison.InvariantCultureIgnoreCase))
              .FirstOrDefault() ?? columnNames.Last();


            var dataView = mlContext.Data.LoadFromTextFile(filePath, options: options);
            var fileName = Path.GetFileNameWithoutExtension(file.FileName);
            var dataViewFileName = $"{fileName}.idv";
            var stream = new MemoryStream();
            mlContext.Data.SaveAsBinary(dataView, stream);

            var dataview = new DataViewFile
            {
                Name = dataViewFileName,
                Size = stream.Length,
                Data = stream.ToArray()
            };

            return dataview;
        }
    }
}
