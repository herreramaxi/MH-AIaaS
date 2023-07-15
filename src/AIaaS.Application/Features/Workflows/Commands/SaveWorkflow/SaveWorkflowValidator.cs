using FluentValidation;

namespace AIaaS.Application.Features.Workflows.Commands
{
    public class SaveWorkflowValidator : AbstractValidator<SaveWorkflowCommand>
    {
        public SaveWorkflowValidator()
        {
            RuleFor(x => x.WorkflowSaveDto).NotNull();
            RuleFor(x => x.WorkflowSaveDto.Id).GreaterThan(0);
            RuleFor(x => x.WorkflowSaveDto.Root).NotEmpty();
        }
    }
}
