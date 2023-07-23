using AIaaS.Domain.Entities;
using Ardalis.Specification;

namespace AIaaS.Application.Specifications.WorkflowRunHistories
{
    public class GetAllWorkflowRunHistoryWithWorkflowNameSpec : Specification<WorkflowRunHistory, WorkflowRunHistory>
    {
        public GetAllWorkflowRunHistoryWithWorkflowNameSpec()
        {
            Query
                .Include(x => x.Workflow)
                .OrderByDescending(x => x.StartDate)
                .ThenBy(x => x.Workflow.Name);

            Query.Select(x => new WorkflowRunHistory
            {
                Id = x.Id,
                WorkflowName = x.Workflow != null ? x.Workflow.Name : null,            
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                Status=x.Status,
                Description = x.Description,
                StatusDetail = x.StatusDetail,
                CreatedBy = x.CreatedBy,
                ModifiedBy = x.ModifiedBy,
                CreatedOn = x.CreatedOn,
                ModifiedOn = x.ModifiedOn,
            });
        }
    }
}
