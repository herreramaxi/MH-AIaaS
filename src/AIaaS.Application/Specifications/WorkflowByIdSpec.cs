using AIaaS.Domain.Entities;
using Ardalis.Specification;

namespace AIaaS.Application.Specifications
{
    public class WorkflowByIdSpec : SingleResultSpecification<Workflow>
    {
        public WorkflowByIdSpec(int workflowId)
        {
            Query
                .Where(x => x.Id == workflowId);
        }
    }
}
