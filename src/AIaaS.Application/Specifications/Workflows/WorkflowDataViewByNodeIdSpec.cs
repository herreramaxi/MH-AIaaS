using AIaaS.Domain.Entities;
using Ardalis.Specification;

namespace AIaaS.Application.Specifications.Workflows
{
    public class WorkflowDataViewByNodeIdSpec : SingleResultSpecification<WorkflowDataView>
    {
        public WorkflowDataViewByNodeIdSpec(string nodeId)
        {
            Query
                .Where(x => x.NodeId.Equals(nodeId, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
