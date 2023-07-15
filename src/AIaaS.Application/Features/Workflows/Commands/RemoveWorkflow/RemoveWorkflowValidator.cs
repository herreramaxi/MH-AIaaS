using FluentValidation;

namespace AIaaS.Application.Features.Workflows.Commands
{
    public class RemoveWorkflowValidator : AbstractValidator<RemoveWorkflowCommand>
    {
        public RemoveWorkflowValidator()
        {
            RuleFor(x => x.WorkflowId).GreaterThan(0);
        }
    }
}
