using AIaaS.Application.Common.Models;
using AIaaS.Application.Common.Models.Dtos;
using AIaaS.Domain.Entities;
using Ardalis.Result;
using AutoMapper;
using CleanArchitecture.Application.Common.Interfaces;
using MediatR;

namespace AIaaS.Application.Workflows.Commands.RenameWorkflow
{
    public class RenameWorkflowCommand : IRequest<Result<WorkflowDto>>
    {
        public RenameWorkflowCommand(WorkflowRenameParameter renameParameter)
        {
            RenameParameter = renameParameter;
        }

        public WorkflowRenameParameter RenameParameter { get; }
    }

    public class RenameWorkflowHandler : IRequestHandler<RenameWorkflowCommand, Result<WorkflowDto>>
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly IMapper _mapper;

        public RenameWorkflowHandler(IApplicationDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }
        public async Task<Result<WorkflowDto>> Handle(RenameWorkflowCommand request, CancellationToken cancellationToken)
        {
            var workflow = await _dbContext.Workflows.FindAsync(request.RenameParameter.Id);

            if (workflow is null) return Result.NotFound();

            workflow.Name = request.RenameParameter.Name;
            workflow.Description = request.RenameParameter.Description;

            _dbContext.Workflows.Update(workflow);
            await _dbContext.SaveChangesAsync();

            var mapped = _mapper.Map<Workflow, WorkflowDto>(workflow);

            return Result.Success(mapped);
        }
    }
}
