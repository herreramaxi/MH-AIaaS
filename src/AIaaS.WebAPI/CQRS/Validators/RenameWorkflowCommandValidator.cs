using AIaaS.WebAPI.CQRS.Commands;
using FluentValidation;

namespace AIaaS.WebAPI.CQRS.Validators
{
    public class RenameWorkflowCommandValidator : AbstractValidator<RenameWorkflowCommand>
    {
        public RenameWorkflowCommandValidator()
        {
            RuleFor(c => c.RenameParameter).NotNull();
            RuleFor(c => c.RenameParameter.Name).NotEmpty();
            RuleFor(c => c.RenameParameter.Id).GreaterThan(0);
        }
    }
}
