using AIaaS.Application.Common.Models;
using AIaaS.Domain.Entities;
using MediatR;

namespace AIaaS.Application.Features.Workflows.Queries.GetLatestWorkflowRunHistory
{
    public class GetLatestWorkflowRunHistoryQuery: IRequest<WorkflowRunHistoryDto?>
    {
        public GetLatestWorkflowRunHistoryQuery(int workflowId)
        {
            WorkflowId = workflowId;
        }

        public int WorkflowId { get; }
    }
}
