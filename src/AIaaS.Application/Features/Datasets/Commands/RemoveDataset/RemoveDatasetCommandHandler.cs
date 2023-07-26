﻿using AIaaS.Application.Specifications.Datasets;
using AIaaS.Domain.Entities;
using AIaaS.Domain.Interfaces;
using Ardalis.Result;
using MediatR;

namespace AIaaS.Application.Features.Datasets.Commands.RemoveDataset
{
    public class RemoveDatasetCommandHandler : IRequestHandler<RemoveDatasetCommand, Result>
    {
        private readonly IRepository<Dataset> _repository;
        private readonly IS3Service _s3Service;

        public RemoveDatasetCommandHandler(IRepository<Dataset> repository, IS3Service s3Service)
        {
            _repository = repository;
            _s3Service = s3Service;
        }

        public async Task<Result> Handle(RemoveDatasetCommand request, CancellationToken cancellationToken)
        {
            var dataset = await _repository.FirstOrDefaultAsync(new DatasetByIdWithFileStorageAndDaviewFileSec(request.DatasetId), cancellationToken);
            if (dataset is null)
            {
                return Result.NotFound("Dataset not found");
            }

            if (dataset.DataViewFile is null)
            {
                return Result.NotFound("DataViewFile not found");
            }

            var deleted = await _s3Service.DeleteFileAsync(dataset.DataViewFile.S3Key);
            if (!deleted)
            {
                return Result.Error("There was an error when trying to delete dataview from S3");
            }

            if (dataset.FileStorage is null)
            {
                return Result.NotFound("FileStorage not found");
            }

            deleted = await _s3Service.DeleteFileAsync(dataset.FileStorage.S3Key);
            if (!deleted)
            {
                return Result.Error("There was an error when trying to delete fileStorage from S3");
            }

            await _repository.DeleteAsync(dataset, cancellationToken);

            return Result.Success();
        }
    }
}
