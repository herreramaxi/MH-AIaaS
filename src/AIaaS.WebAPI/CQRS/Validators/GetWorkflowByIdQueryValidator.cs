using AIaaS.WebAPI.CQRS.Queries;
using FluentValidation;

namespace AIaaS.WebAPI.CQRS.Validators
{
    public class GetWorkflowByIdQueryValidator : AbstractValidator<GetWorkflowByIdQuery>
    {
        public GetWorkflowByIdQueryValidator()
        {
            RuleFor(x => x.WorkflowId).GreaterThan(0);
        }
    }
}
