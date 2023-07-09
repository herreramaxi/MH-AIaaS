using AIaaS.WebAPI.CQRS.Commands;
using FluentValidation;

namespace AIaaS.WebAPI.CQRS.Validators
{
    public class ValidateWorkflowCommandValidator : AbstractValidator<ValidateWorkflowCommand>
    {
        public ValidateWorkflowCommandValidator()
        {
            RuleFor(x => x.WorkflowDto).NotNull();
            RuleFor(x => x.WorkflowDto.Id).GreaterThan(0);
            RuleFor(x => x.WorkflowDto.Root).NotEmpty();
        }
    }
}
