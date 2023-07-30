using AIaaS.Domain.Entities;
using Ardalis.Specification;

namespace AIaaS.Application.Specifications.Workflows
{
    public class WorkflowDataViewByNodeIdSpec : SingleResultSpecification<WorkflowDataView>
    {
        public WorkflowDataViewByNodeIdSpec(Guid nodeGuid)
        {
            Query
                .Where(x => x.NodeGuid.Equals(nodeGuid));
        }
    }
}
