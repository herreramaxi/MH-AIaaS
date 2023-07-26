using AIaaS.Application.Common.Models;
using AIaaS.Application.Specifications.Datasets;
using AIaaS.Domain.Entities;
using AIaaS.Domain.Interfaces;
using Ardalis.Result;
using AutoMapper;
using MediatR;

namespace AIaaS.Application.Features.Datasets.Queries.GetDatasetFileStorage
{
    public class GetDatasetFileStorageHandler : IRequestHandler<GetDatasetFileStorageRequest, Result<FileStorageDto>>
    {
        private readonly IReadRepository<Dataset> _datasetRepository;
        private readonly IS3Service _s3Service;
        private readonly IMapper _mapper;

        public GetDatasetFileStorageHandler(IReadRepository<Dataset> datasetRepository, IS3Service s3Service, IMapper mapper)
        {
            _datasetRepository = datasetRepository;
            _s3Service = s3Service;
            _mapper = mapper;
        }
        public async Task<Result<FileStorageDto>> Handle(GetDatasetFileStorageRequest request, CancellationToken cancellationToken)
        {
            var dataset = await _datasetRepository.FirstOrDefaultAsync(new DatasetByIdWithFileStorageSpec(request.DatasetId), cancellationToken);
            if (dataset is null)
            {
                return Result.NotFound();
            }

            var mapped = _mapper.Map<FileStorageDto>(dataset?.FileStorage);
            mapped.FileStream = await _s3Service.DownloadFileAsync(mapped.S3Key);

            if (mapped.FileStream is null)
            {
                return Result.Error("Not able to download file from S3");
            }

            return Result.Success(mapped);
        }
    }
}
