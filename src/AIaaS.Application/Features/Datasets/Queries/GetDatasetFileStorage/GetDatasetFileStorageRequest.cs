using AIaaS.Application.Common.Models;
using MediatR;

namespace AIaaS.Application.Features.Datasets.Queries.GetDatasetFileStorage
{
    public class GetDatasetFileStorageRequest: IRequest<FileStorageDto?>
    {
        public GetDatasetFileStorageRequest(int datasetId)
        {
            DatasetId = datasetId;
        }

        public int DatasetId { get; }
    }
}
