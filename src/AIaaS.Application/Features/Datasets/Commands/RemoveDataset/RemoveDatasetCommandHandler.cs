using AIaaS.Application.Specifications.Datasets;
using AIaaS.Domain.Entities;
using AIaaS.Domain.Interfaces;
using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIaaS.Application.Features.Datasets.Commands.RemoveDataset
{
    public class RemoveDatasetCommandHandler : IRequestHandler<RemoveDatasetCommand, Result>
    {
        private readonly IRepository<Dataset> _repository;
        private readonly IS3Service _s3Service;
        private readonly ILogger<RemoveDatasetCommandHandler> _logger;

        public RemoveDatasetCommandHandler(IRepository<Dataset> repository, IS3Service s3Service, ILogger<RemoveDatasetCommandHandler> logger)
        {
            _repository = repository;
            _s3Service = s3Service;
            _logger = logger;
        }

        public async Task<Result> Handle(RemoveDatasetCommand request, CancellationToken cancellationToken)
        {
            var dataset = await _repository.FirstOrDefaultAsync(new DatasetByIdWithFileStorageAndDaviewFileSec(request.DatasetId), cancellationToken);
            if (dataset is null)
            {
                return Result.NotFound("Dataset not found");
            }

            if (dataset.DataViewFile is not null)
            {
                var deleted = await _s3Service.DeleteFileAsync(dataset.DataViewFile.S3Key);
                if (!deleted)
                {
                    _logger.LogWarning("Not able to delete dataview file from S3");
                }
            }

            if (dataset.FileStorage is not null)
            {
                var deleted = await _s3Service.DeleteFileAsync(dataset.FileStorage.S3Key);
                if (!deleted)
                {
                    _logger.LogWarning("Not able to delete file storage from S3");
                }
            }

            await _repository.DeleteAsync(dataset, cancellationToken);

            return Result.Success();
        }
    }
}
