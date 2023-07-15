
using AIaaS.Application.Common.Models;
using AIaaS.Domain.Entities;
using AIaaS.Domain.Interfaces;
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
        private readonly IRepository<Workflow> _workflowRepository;
        private readonly IMapper _mapper;

        public CreateWorkflowHandler(IRepository<Workflow> workflowRepository, IMapper mapper)
        {
            _workflowRepository = workflowRepository;
            _mapper = mapper;
        }
        public async Task<WorkflowDto> Handle(CreateWorkflowCommand request, CancellationToken cancellationToken)
        {
            var workflow = new Workflow()
            {
                Name = request.WorkflowName
            };

            await _workflowRepository.AddAsync(workflow, cancellationToken);

            var mapped = _mapper.Map<Workflow, WorkflowDto>(workflow);
            return mapped;
        }
    }
}
