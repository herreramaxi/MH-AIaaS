using FluentValidation;

namespace AIaaS.Application.Workflows.Queries.GetWorkflowById
{
    public class GetWorkflowByIdValidator : AbstractValidator<GetWorkflowByIdQuery>
    {
        public GetWorkflowByIdValidator()
        {
            RuleFor(x => x.WorkflowId).GreaterThan(0);
        }
    }
}
