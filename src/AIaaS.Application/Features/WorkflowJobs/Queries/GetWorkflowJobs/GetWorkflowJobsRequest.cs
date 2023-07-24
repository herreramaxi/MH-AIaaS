using AIaaS.Application.Common.Models;
using MediatR;

namespace AIaaS.Application.Features.WorkflowJobs.Queries.GetWorkflowJobs
{
    public class GetWorkflowJobsRequest: IRequest<IList<WorkflowRunHistoryDto>>
    {
        public GetWorkflowJobsRequest(int? workflowId)
        {
            WorkflowId = workflowId;
        }

        public int? WorkflowId { get; }
    }
}
