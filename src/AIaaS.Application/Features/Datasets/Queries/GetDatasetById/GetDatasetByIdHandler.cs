using AIaaS.Application.Common.Models;
using AIaaS.Application.Specifications.Datasets;
using AIaaS.Domain.Entities;
using AIaaS.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace AIaaS.Application.Features.Datasets.Queries
{
    public class GetDatasetByIdHandler : IRequestHandler<GetDatasetByIdRequest, DatasetDto?>
    {
        private readonly IReadRepository<Dataset> _datasetRepository;
        private readonly IS3Service _s3Service;
        private readonly IMapper _mapper;

        public GetDatasetByIdHandler(IReadRepository<Dataset> datasetRepository, IS3Service s3Service,IMapper mapper)
        {
            _datasetRepository = datasetRepository;
            _s3Service = s3Service;
            _mapper = mapper;
        }
        public async Task<DatasetDto?> Handle(GetDatasetByIdRequest request, CancellationToken cancellationToken)
        {
            var dataset = await _datasetRepository.FirstOrDefaultAsync(new DatasetByIdWithFileStorageAndDaviewFileSec(request.DatasetId), cancellationToken);
            var datasetDto = _mapper.Map<DatasetDto>(dataset);
            datasetDto.FileUrl = _s3Service.GetS3ResourceUrl(dataset?.FileStorage?.S3Key);
            datasetDto.DataViewFileUrl = _s3Service.GetS3ResourceUrl(dataset?.DataViewFile?.S3Key);

            return datasetDto;
        }
    }
}
