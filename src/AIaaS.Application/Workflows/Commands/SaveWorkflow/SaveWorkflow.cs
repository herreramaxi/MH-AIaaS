
using AIaaS.Application.Common.Models;
using AIaaS.Domain.Entities;
using Ardalis.Result;
using AutoMapper;
using CleanArchitecture.Application.Common.Interfaces;
using MediatR;

namespace AIaaS.Application.Workflows.Commands.SaveWorkflow
{
    public class SaveWorkflowCommand : IRequest<Result<WorkflowDto>>
    {
        public WorkflowSaveDto WorkflowSaveDto { get; }
        public SaveWorkflowCommand(WorkflowSaveDto workflowSaveDto)
        {
            WorkflowSaveDto = workflowSaveDto;
        }
    }

    public class SaveWorkflowHandler : IRequestHandler<SaveWorkflowCommand, Result<WorkflowDto>>
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly IMapper _mapper;

        public SaveWorkflowHandler(IApplicationDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }
        public async Task<Result<WorkflowDto>> Handle(SaveWorkflowCommand request, CancellationToken cancellationToken)
        {
            var workflowDto = request.WorkflowSaveDto;
            var workflow = await _dbContext.Workflows.FindAsync(workflowDto.Id);

            if (workflow == null)
                return Result.NotFound();

            workflow.Data = workflowDto.Root;

            _dbContext.Workflows.Update(workflow);
            await _dbContext.SaveChangesAsync();

            var mapped = _mapper.Map<Workflow, WorkflowDto>(workflow);

            return Result.Success(mapped);
        }
    }
}
