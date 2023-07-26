using AIaaS.Application.Common.Models;
using AIaaS.Domain.Entities;
using MediatR;

namespace AIaaS.Application.Features.Datasets.Commands.CreateDataset
{
    public class CreateDatasetCommand : IRequest<Dataset>
    {
        public CreateDatasetCommand(CreateDatasetParameter createDatasetParameter)
        {
            CreateDatasetParameter = createDatasetParameter;
        }

        public CreateDatasetParameter CreateDatasetParameter { get; }
    }
}
