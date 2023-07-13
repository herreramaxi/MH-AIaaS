
using AIaaS.Application.Common.Models;
using AIaaS.Domain.Entities;
using AutoMapper;
using CleanArchitecture.Application.Common.Interfaces;
using MediatR;

namespace AIaaS.Application.Workflows.Commands.CreateWorkflow
{
    public class CreateWorkflowCommand : IRequest<WorkflowDto>
    {
        public string WorkflowName { get; set; } = $"Workflow-created ({DateTime.Now})";
    }
    public class CreateWorkflowHandler : IRequestHandler<CreateWorkflowCommand, WorkflowDto>
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly IMapper _mapper;

        public CreateWorkflowHandler(IApplicationDbContext dbContext, IMapper mapper)
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
            await _dbContext.SaveChangesAsync(cancellationToken);

            var mapped = _mapper.Map<Workflow, WorkflowDto>(workflow);
            return mapped;
        }
    }
}
