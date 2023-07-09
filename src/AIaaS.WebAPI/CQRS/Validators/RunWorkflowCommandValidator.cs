using AIaaS.WebAPI.CQRS.Commands;
using FluentValidation;

namespace AIaaS.WebAPI.CQRS.Validators
{
    public class RunWorkflowCommandValidator: AbstractValidator<RunWorkflowCommand>
    {
        public RunWorkflowCommandValidator()
        {
            RuleFor(x => x.WorkflowDto).NotNull();
            RuleFor(x => x.WorkflowDto.Id).GreaterThan(0);
            RuleFor(x => x.WorkflowDto.Root).NotEmpty();
        }
    }
}
