using AIaaS.Application.Common.Models;
using MediatR;

namespace AIaaS.Application.Features.Datasets.Queries
{
    public class GetDataViewFileRequest : IRequest<DataViewFileDto?>
    {
        public GetDataViewFileRequest(int datasetId)
        {
            DatasetId = datasetId;
        }

        public int DatasetId { get; }
    }
}
