using AIaaS.WebAPI.CQRS.Commands;
using AIaaS.WebAPI.Data;
using AIaaS.WebAPI.Models;
using Ardalis.Result;
using AutoMapper;
using MediatR;

namespace AIaaS.WebAPI.CQRS.Handlers
{
    public class SaveWorkflowHandler : IRequestHandler<SaveWorkflowCommand, Result<WorkflowDto>>
    {
        private readonly EfContext _dbContext;
        private readonly IMapper _mapper;

        public SaveWorkflowHandler(EfContext dbContext, IMapper mapper)
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
