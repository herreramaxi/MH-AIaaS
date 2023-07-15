using Ardalis.Result;
using MediatR;

namespace AIaaS.Application.Features.Datasets.Commands.RemoveDataset
{
    public class RemoveDatasetCommand: IRequest<Result>
    {
        public RemoveDatasetCommand(int datasetId)
        {
            DatasetId = datasetId;
        }

        public int DatasetId { get; }
    }
}
