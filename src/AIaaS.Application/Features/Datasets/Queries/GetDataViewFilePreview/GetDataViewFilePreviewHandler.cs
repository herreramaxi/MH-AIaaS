using AIaaS.Application.Common.Models;
using AIaaS.Application.Specifications.Datasets;
using AIaaS.Domain.Entities;
using AIaaS.Domain.Interfaces;
using AIaaS.WebAPI.ExtensionMethods;
using MediatR;

namespace AIaaS.Application.Features.Datasets.Queries.GetDataViewFilePreview
{
    public class GetDataViewFilePreviewHandler : IRequestHandler<GetDataViewFilePreviewRequest, DataViewFilePreviewDto?>
    {
        private readonly IReadRepository<Dataset> _datasetRepository;

        public GetDataViewFilePreviewHandler(IReadRepository<Dataset> datasetRepository)
        {
            _datasetRepository = datasetRepository;
        }

        public async Task<DataViewFilePreviewDto?> Handle(GetDataViewFilePreviewRequest request, CancellationToken cancellationToken)
        {
            var dataset = await _datasetRepository.FirstOrDefaultAsync(new DatasetByIdWithDataViewFileSpec(request.DatasetId), cancellationToken);
            if (dataset?.DataViewFile?.Data is null) return null;

            var dataPreview = dataset.DataViewFile.Data.GetPreview();

            return dataPreview;
        }
    }
}
