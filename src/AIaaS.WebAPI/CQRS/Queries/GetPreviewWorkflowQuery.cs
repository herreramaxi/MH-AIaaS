using AIaaS.WebAPI.Models;
using Ardalis.Result;
using MediatR;

namespace AIaaS.WebAPI.CQRS.Queries
{
    public class GetPreviewWorkflowQuery : IRequest<Result<object>>
    {
        public GetPreviewWorkflowQuery(int workflowDataviewId)
        {
            WorkflowDataviewId = workflowDataviewId;
        }

        public int WorkflowDataviewId { get; }
    }
}
