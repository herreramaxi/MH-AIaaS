using FluentValidation;

namespace AIaaS.Application.Workflows.Queries.GetPreviewWorkflow
{
    public class GetPreviewWorkflowValidator : AbstractValidator<GetPreviewWorkflowQuery>
    {
        public GetPreviewWorkflowValidator()
        {
            RuleFor(x => x.WorkflowDataviewId).GreaterThan(0);
        }
    }
}
