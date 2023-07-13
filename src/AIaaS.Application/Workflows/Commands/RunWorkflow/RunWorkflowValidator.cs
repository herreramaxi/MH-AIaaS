using FluentValidation;

namespace AIaaS.Application.Workflows.Commands.RunWorkflow
{
    public class RunWorkflowValidator : AbstractValidator<RunWorkflowCommand>
    {
        public RunWorkflowValidator()
        {
            RuleFor(x => x.WorkflowDto).NotNull();
            RuleFor(x => x.WorkflowDto.Id).GreaterThan(0);
            RuleFor(x => x.WorkflowDto.Root).NotEmpty();
        }
    }
}
