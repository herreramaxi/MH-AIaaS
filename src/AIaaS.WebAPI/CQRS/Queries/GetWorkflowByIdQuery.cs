using AIaaS.WebAPI.Models;
using Ardalis.Result;
using MediatR;

namespace AIaaS.WebAPI.CQRS.Queries
{
    public class GetWorkflowByIdQuery : IRequest<Result<WorkflowDto>>
    {
        public int WorkflowId { get; }
        public GetWorkflowByIdQuery(int workflowId)
        {
            WorkflowId = workflowId;
        }
    }
}
