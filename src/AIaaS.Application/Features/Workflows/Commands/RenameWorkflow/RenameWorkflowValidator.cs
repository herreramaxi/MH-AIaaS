﻿using FluentValidation;

namespace AIaaS.Application.Features.Workflows.Commands
{
    public class RenameWorkflowValidator : AbstractValidator<RenameWorkflowCommand>
    {
        public RenameWorkflowValidator()
        {
            RuleFor(c => c.RenameParameter).NotNull();
            RuleFor(c => c.RenameParameter.Name).NotEmpty();
            RuleFor(c => c.RenameParameter.Id).GreaterThan(0);
        }
    }
}