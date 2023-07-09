using AIaaS.WebAPI.CQRS.Commands;
using FluentValidation;

namespace AIaaS.WebAPI.CQRS.Validators
{
    public class SaveWorkflowCommandValidator : AbstractValidator<SaveWorkflowCommand>
    {
        public SaveWorkflowCommandValidator()
        {
            RuleFor(x => x.WorkflowSaveDto).NotNull();
            RuleFor(x => x.WorkflowSaveDto.Id).GreaterThan(0);
            RuleFor(x => x.WorkflowSaveDto.Root).NotEmpty();
        }
    }
}
