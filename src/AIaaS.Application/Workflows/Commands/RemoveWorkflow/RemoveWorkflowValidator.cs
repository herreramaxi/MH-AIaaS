using FluentValidation;

namespace AIaaS.Application.Workflows.Commands.RemoveWorkflow
{
    public class RemoveWorkflowValidator : AbstractValidator<RemoveWorkflowCommand>
    {
        public RemoveWorkflowValidator()
        {
            RuleFor(x => x.WorkflowId).GreaterThan(0);
        }
    }
}
