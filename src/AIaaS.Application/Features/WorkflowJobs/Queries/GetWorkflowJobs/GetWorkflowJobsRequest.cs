using AIaaS.Application.Common.Models;
using MediatR;

namespace AIaaS.Application.Features.WorkflowJobs.Queries.GetWorkflowJobs
{
    public class GetWorkflowJobsRequest: IRequest<IList<WorkflowRunHistoryDto>>
    {
    }
}
