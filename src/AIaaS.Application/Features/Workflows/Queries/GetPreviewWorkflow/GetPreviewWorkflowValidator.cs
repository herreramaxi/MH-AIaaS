using FluentValidation;

namespace AIaaS.Application.Features.Workflows.Queries
{
    public class GetPreviewWorkflowValidator : AbstractValidator<GetPreviewWorkflowQuery>
    {
        public GetPreviewWorkflowValidator()
        {
            RuleFor(x => x.WorkflowDataviewId).GreaterThan(0);
        }
    }
}
