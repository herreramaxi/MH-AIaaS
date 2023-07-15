using AIaaS.Application.Common.Models;
using MediatR;

namespace AIaaS.Application.Features.Datasets.Queries
{
    public class GetDatasetByIdRequest : IRequest<DatasetDto?>
    {
        public GetDatasetByIdRequest(int datasetId)
        {
            DatasetId = datasetId;
        }

        public int DatasetId { get; }
    }
}
