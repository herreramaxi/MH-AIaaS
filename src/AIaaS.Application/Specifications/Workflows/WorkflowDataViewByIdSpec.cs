using AIaaS.Domain.Entities;
using Ardalis.Specification;

namespace AIaaS.Application.Specifications.Workflows
{
    public class WorkflowDataViewByIdSpec : SingleResultSpecification<WorkflowDataView>
    {
        public WorkflowDataViewByIdSpec(int workflowDataViewId)
        {
            Query
                .Where(x =>  x.Id == workflowDataViewId)
                .EnableCache(nameof(WorkflowDataViewByIdSpec), workflowDataViewId);
        }
    }
}
