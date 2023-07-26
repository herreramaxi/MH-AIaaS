using AIaaS.Application.Common.Models;
using Ardalis.Result;
using MediatR;

namespace AIaaS.Application.Features.Datasets.Queries
{
    public class GetDataViewFilePreviewRequest : IRequest<Result<DataViewFilePreviewDto>>
    {
        public GetDataViewFilePreviewRequest(int datasetId)
        {
            DatasetId = datasetId;
        }

        public int DatasetId { get; }
    }
}
