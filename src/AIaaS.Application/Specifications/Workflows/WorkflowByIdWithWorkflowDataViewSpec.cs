using AIaaS.Domain.Entities;
using Ardalis.Specification;

namespace AIaaS.Application.Specifications.Workflows
{
    public class WorkflowByIdWithWorkflowDataViewSpec : SingleResultSpecification<Workflow>
    {
        public WorkflowByIdWithWorkflowDataViewSpec(int workflowId)
        {
            Query
                .Where(x => x.Id == workflowId)
                .Include(x => x.WorkflowDataViews);
        }
    }
}
