using AIaaS.Application.Common.Models;
using AIaaS.Application.Specifications.Datasets;
using AIaaS.Domain.Entities;
using AIaaS.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace AIaaS.Application.Features.Datasets.Queries
{
    public class GetDatasetColumnsHandler : IRequestHandler<GetDatasetColumnsRequest, DatasetDto?>
    {
        private readonly IReadRepository<Dataset> _datasetRepository;
        private readonly IMapper _mapper;

        public GetDatasetColumnsHandler(IReadRepository<Dataset> datasetRepository, IMapper mapper)
        {
            _datasetRepository = datasetRepository;
            _mapper = mapper;
        }

        public async Task<DatasetDto?> Handle(GetDatasetColumnsRequest request, CancellationToken cancellationToken)
        {
            var dataset = await _datasetRepository.FirstOrDefaultAsync(new DatasetByIdWithColumnSettingsSpec(request.DatasetId), cancellationToken);
            var mapped = _mapper.Map<DatasetDto>(dataset);
                       
            return mapped;
        }
    }
}
