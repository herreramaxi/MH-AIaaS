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
        private readonly ILogger<UploadFileStorageCommandHandler> _logger;
        private readonly IRepository<Dataset> _repository;
        private readonly IS3Service _s3Service;

        public UploadFileStorageCommandHandler(IRepository<Dataset> repository, IS3Service s3Service, ILogger<UploadFileStorageCommandHandler> logger)
        {
            _repository = repository;
            _s3Service = s3Service;
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

                dataset.FileStorage = new FileStorage()
                {
                    FileName = file.FileName,
                    Size = file.Length,
                    S3Key = file.FileName.GenerateS3Key()
                };

                var uploadedToS3 = await _s3Service.UploadFileAsync(reader, dataset.FileStorage.S3Key, true);
                if (!uploadedToS3)
                {
                    return Result.Error("Error when trying to upload the file to S3");
                }

                var createDataViewResult = await CreateDataViewFileAsync(filePath, dataset, file);

                if (!createDataViewResult.IsSuccess || createDataViewResult.Value is null)
                {
                    return Result.Error(createDataViewResult.Errors.FirstOrDefault() ?? "Error when trying to create dataview file");
                }

                dataset.DataViewFile = createDataViewResult.Value;

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

        private async Task<Result<DataViewFile>> CreateDataViewFileAsync(string? filePath, Dataset dataset, IFormFile file)
        {
            try
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
                    S3Key = dataViewFileName.GenerateS3Key()
                };

                var uploadedToS3 = await _s3Service.UploadFileAsync(stream, dataview.S3Key, true);
                if (!uploadedToS3)
                {
                    return Result.Error("Not able to upload dataview file to S3");
                }

                return Result.Success(dataview);
            }
            catch (Exception exception)
            {
                var errorMessage = "Error when trying to generate dataview file";
                _logger.LogError(exception, errorMessage);

                return Result.Error(errorMessage);
            }
        }
    }
}
