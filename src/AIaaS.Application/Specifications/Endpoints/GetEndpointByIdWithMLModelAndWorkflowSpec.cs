using AIaaS.Domain.Entities;
using Ardalis.Specification;
using Microsoft.EntityFrameworkCore;

namespace AIaaS.Application.Specifications.Endpoints
{
    public class GetEndpointByIdWithMLModelAndWorkflowSpec: SingleResultSpecification<MLEndpoint>
    {
        public GetEndpointByIdWithMLModelAndWorkflowSpec(int endpointId)
        {
            Query
                .Include(e => e.MLModel)
                    .ThenInclude(m => m.Workflow)
                .Where(x => x.Id == endpointId);
        }
    }
}
