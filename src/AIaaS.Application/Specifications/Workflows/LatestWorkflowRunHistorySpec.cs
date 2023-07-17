using AIaaS.Domain.Entities;
using Ardalis.Specification;

namespace AIaaS.Application.Specifications.Workflows
{
    public class LatestWorkflowRunHistorySpec : SingleResultSpecification<WorkflowRunHistory>
    {
        public LatestWorkflowRunHistorySpec(int workflowId)
        {
            Query
                .Take(1)
                .Where(x => x.WorkflowId == workflowId)
                .OrderByDescending(x => x.Id);                
        }
    }
}
