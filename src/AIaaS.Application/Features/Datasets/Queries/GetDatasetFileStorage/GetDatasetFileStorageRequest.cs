using AIaaS.Application.Common.Models;
using Ardalis.Result;
using MediatR;

namespace AIaaS.Application.Features.Datasets.Queries.GetDatasetFileStorage
{
    public class GetDatasetFileStorageRequest: IRequest<Result<FileStorageDto>>
    {
        public GetDatasetFileStorageRequest(int datasetId)
        {
            DatasetId = datasetId;
        }

        public int DatasetId { get; }
    }
}
