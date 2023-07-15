using AIaaS.Domain.Entities;
using Ardalis.Specification;

namespace AIaaS.Application.Specifications.Workflows
{
    public class WorkflowByIdWithModelSpec : SingleResultSpecification<Workflow>
    {
        public WorkflowByIdWithModelSpec(int workflowId)
        {
            Query
                .Where(x => x.Id == workflowId)
                .Include(x => x.MLModel);
        }
    }
}
