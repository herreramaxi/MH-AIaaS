using AIaaS.Application.Common.Models;
using MediatR;

namespace AIaaS.Application.Features.WorkflowJobs.Queries.GetWorkflowJobs
{
    public class GetWorkflowJobDetailsRequest : IRequest<IList<WorkflowNodeRunHistoryDto>>
    {
        public GetWorkflowJobDetailsRequest(int workflowRunId)
        {
            WorkflowRunId = workflowRunId;
        }

        public int WorkflowRunId { get; }
    }
}
