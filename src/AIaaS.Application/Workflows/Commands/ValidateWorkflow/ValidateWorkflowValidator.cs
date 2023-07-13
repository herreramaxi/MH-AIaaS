using FluentValidation;

namespace AIaaS.Application.Workflows.Commands.ValidateWorkflow
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
