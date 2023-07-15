using AIaaS.Application.Common.Models;
using AIaaS.Application.Specifications.Datasets;
using AIaaS.Domain.Entities;
using AIaaS.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace AIaaS.Application.Features.Datasets.Queries
{
    public class GetAllDatasetHandler : IRequestHandler<GetAllDatasetRequest, IEnumerable<DatasetDto>>
    {
        private readonly IReadRepository<Dataset> _datasetRepository;
        private readonly IMapper _mapper;

        public GetAllDatasetHandler(IReadRepository<Dataset> datasetRepository, IMapper mapper)
        {
            _datasetRepository = datasetRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<DatasetDto>> Handle(GetAllDatasetRequest request, CancellationToken cancellationToken)
        {
            var datasets = await _datasetRepository.ListAsync(new DatasetGetAllSpec(), cancellationToken);
            var dtos = _mapper.Map<IEnumerable<DatasetDto>>(datasets);

            return dtos;
        }
    }
}
