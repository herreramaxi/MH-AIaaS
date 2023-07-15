using AIaaS.Domain.Entities;
using Ardalis.Specification;

namespace AIaaS.Application.Specifications
{
    public class WorkflowByIdIncludeAllSpec : SingleResultSpecification<Workflow>
    {
        public WorkflowByIdIncludeAllSpec(int workflowId)
        {
            Query
                .Where(x => x.Id == workflowId)
                .Include(x => x.MLModel)
                .ThenInclude(x => x.ModelMetrics)
                .Include(x => x.WorkflowDataViews);
        }
    }
}
