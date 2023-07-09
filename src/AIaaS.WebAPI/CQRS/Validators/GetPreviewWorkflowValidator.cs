using AIaaS.WebAPI.CQRS.Queries;
using FluentValidation;

namespace AIaaS.WebAPI.CQRS.Validators
{
    public class GetPreviewWorkflowValidator : AbstractValidator<GetPreviewWorkflowQuery>
    {
        public GetPreviewWorkflowValidator()
        {
            RuleFor(x => x.WorkflowDataviewId).GreaterThan(0);
        }
    }
}
