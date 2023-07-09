using AIaaS.WebAPI.Models;
using MediatR;

namespace AIaaS.WebAPI.CQRS.Queries
{
    public class GetAllWorkflowsQuery : IRequest<IList<WorkflowItemDto>>
    {
    }
}
