using FluentValidation;

namespace AIaaS.Application.Features.Workflows.Queries
{
    public class GetWorkflowByIdValidator : AbstractValidator<GetWorkflowByIdQuery>
    {
        public GetWorkflowByIdValidator()
        {
            RuleFor(x => x.WorkflowId).GreaterThan(0);
        }
    }
}
