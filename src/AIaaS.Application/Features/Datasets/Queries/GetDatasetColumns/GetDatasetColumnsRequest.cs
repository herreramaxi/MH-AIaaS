using AIaaS.Application.Common.Models;
using MediatR;

namespace AIaaS.Application.Features.Datasets.Queries
{
    public class GetDatasetColumnsRequest : IRequest<DatasetDto?>
    {
        public GetDatasetColumnsRequest(int datasetId)
        {
            DatasetId = datasetId;
        }

        public int DatasetId { get; }
    }
}
