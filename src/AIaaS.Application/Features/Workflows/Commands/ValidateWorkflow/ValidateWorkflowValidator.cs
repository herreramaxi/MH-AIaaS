using FluentValidation;

namespace AIaaS.Application.Features.Workflows.Commands
{
    public class ValidateWorkflowValidator : AbstractValidator<ValidateWorkflowCommand>
    {
        public ValidateWorkflowValidator()
        {
            RuleFor(x => x.WorkflowDto).NotNull();
            RuleFor(x => x.WorkflowDto.Id).GreaterThan(0);
            RuleFor(x => x.WorkflowDto.Root).NotEmpty();
        }
    }
}
