using AIaaS.Application.Common.Models;
using MediatR;

namespace AIaaS.Application.Features.Datasets.Queries
{
    public class GetDataViewFilePreviewRequest : IRequest<DataViewFilePreviewDto?>
    {
        public GetDataViewFilePreviewRequest(int datasetId)
        {
            DatasetId = datasetId;
        }

        public int DatasetId { get; }
    }
}
