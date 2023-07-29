using AIaaS.Domain.Entities;
using Ardalis.Specification;

namespace AIaaS.Application.Specifications.WorkflowRunHistories
{
    public class GetWorkflowNodeRunHistoriesByRunId : Specification<WorkflowNodeRunHistory>
    {
        public GetWorkflowNodeRunHistoriesByRunId(int workflowRunHistoryId)
        {
            Query
                .Where(x => x.WorkflowRunHistoryId == workflowRunHistoryId)
                .OrderBy(x => x.StartDate);
        }
    }
}
