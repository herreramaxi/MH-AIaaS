using AIaaS.Application.Common.Models;
using AIaaS.Application.Specifications.Datasets;
using AIaaS.Domain.Entities;
using AIaaS.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace AIaaS.Application.Features.Datasets.Queries.GetDatasetFileStorage
{
    public class GetDatasetFileStorageHandler : IRequestHandler<GetDatasetFileStorageRequest, FileStorageDto?>
    {
        private readonly IReadRepository<Dataset> _datasetRepository;
        private readonly IMapper _mapper;

        public GetDatasetFileStorageHandler(IReadRepository<Dataset> datasetRepository, IMapper mapper)
        {
            _datasetRepository = datasetRepository;
            _mapper = mapper;
        }
        public async Task<FileStorageDto?> Handle(GetDatasetFileStorageRequest request, CancellationToken cancellationToken)
        {
            var dataset = await _datasetRepository.FirstOrDefaultAsync(new DatasetByIdWithFileStorageSpec(request.DatasetId), cancellationToken);
            var mapped = _mapper.Map<FileStorageDto>(dataset?.FileStorage);

            return mapped;
        }
    }
}
