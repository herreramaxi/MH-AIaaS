using AIaaS.Application.Common.Models;
using MediatR;

namespace AIaaS.Application.Features.Datasets.Queries
{
    public class GetAllDatasetRequest : IRequest<IEnumerable<DatasetDto>>
    {
    }
}
