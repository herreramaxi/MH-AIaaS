using AIaaS.Application.Common.Models;
using AIaaS.Application.Interfaces;
using AIaaS.Application.Specifications.Datasets;
using AIaaS.Domain.Entities;
using AIaaS.Domain.Interfaces;
using Ardalis.Result;
using MediatR;

namespace AIaaS.Application.Features.Datasets.Queries.GetDataViewFilePreview
{
    public class GetDataViewFilePreviewHandler : IRequestHandler<GetDataViewFilePreviewRequest, Result<DataViewFilePreviewDto>>
    {
        private readonly IReadRepository<Dataset> _datasetRepository;
        private readonly IDataViewService _dataViewService;

        public GetDataViewFilePreviewHandler(IReadRepository<Dataset> datasetRepository, IDataViewService dataViewService)
        {
            _datasetRepository = datasetRepository;
            _dataViewService = dataViewService;
        }

        public async Task<Result<DataViewFilePreviewDto>> Handle(GetDataViewFilePreviewRequest request, CancellationToken cancellationToken)
        {
            var dataset = await _datasetRepository.FirstOrDefaultAsync(new DatasetByIdWithDataViewFileSpec(request.DatasetId), cancellationToken);
            if (dataset?.DataViewFile is null) return Result.NotFound();

            var result = await _dataViewService.GetPreviewAsync(dataset.DataViewFile);

            return result;
        }
    }
}
