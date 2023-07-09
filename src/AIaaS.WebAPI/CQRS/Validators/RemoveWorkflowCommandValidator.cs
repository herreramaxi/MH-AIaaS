using AIaaS.WebAPI.CQRS.Commands;
using FluentValidation;

namespace AIaaS.WebAPI.CQRS.Validators
{
    public class RemoveWorkflowCommandValidator: AbstractValidator<RemoveWorkflowCommand>
    {
        public RemoveWorkflowCommandValidator()
        {
            RuleFor(x => x.WorkflowId).GreaterThan(0);
        }
    }
}
