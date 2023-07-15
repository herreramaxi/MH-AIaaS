using AIaaS.Application.Common.Models;
using AIaaS.Application.Specifications.Datasets;
using AIaaS.Domain.Entities;
using AIaaS.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace AIaaS.Application.Features.Datasets.Queries.GetDataViewFilePreview
{
    public class GetDataViewFileHandler : IRequestHandler<GetDataViewFileRequest, DataViewFileDto?>
    {
        private readonly IReadRepository<Dataset> _datasetRepository;
        private readonly IMapper _mapper;

        public GetDataViewFileHandler(IReadRepository<Dataset> datasetRepository, IMapper mapper)
        {
            _datasetRepository = datasetRepository;
            _mapper = mapper;
        }

        public async Task<DataViewFileDto?> Handle(GetDataViewFileRequest request, CancellationToken cancellationToken)
        {
            var dataset = await _datasetRepository.FirstOrDefaultAsync(new DatasetByIdWithDataViewFileSpec(request.DatasetId), cancellationToken);
            var mapped = _mapper.Map<DataViewFileDto>(dataset?.DataViewFile);

            return mapped;
        }
    }
}
