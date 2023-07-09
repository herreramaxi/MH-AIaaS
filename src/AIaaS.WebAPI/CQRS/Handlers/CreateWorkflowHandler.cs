using AIaaS.WebAPI.CQRS.Commands;
using AIaaS.WebAPI.Data;
using AIaaS.WebAPI.Models;
using AutoMapper;
using MediatR;

namespace AIaaS.WebAPI.CQRS.Handlers
{
    public class CreateWorkflowHandler : IRequestHandler<CreateWorkflowCommand, WorkflowDto>
    {
        private readonly EfContext _dbContext;
        private readonly IMapper _mapper;

        public CreateWorkflowHandler(EfContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }
        public async Task<WorkflowDto> Handle(CreateWorkflowCommand request, CancellationToken cancellationToken)
        {
            var workflow = new Workflow()
            {
                Name = request.WorkflowName
            };

            await _dbContext.Workflows.AddAsync(workflow);
            await _dbContext.SaveChangesAsync();

            var mapped = _mapper.Map<Workflow, WorkflowDto>(workflow);
            return mapped;
        }
    }
}
