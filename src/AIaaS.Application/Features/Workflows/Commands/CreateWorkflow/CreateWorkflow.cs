using AIaaS.Application.Common.Models;
using AIaaS.Application.Interfaces;
using AIaaS.Domain.Entities;
using AIaaS.Domain.Interfaces;
using Ardalis.Result;
using AutoMapper;
using MediatR;

namespace AIaaS.Application.Features.Workflows.Commands
{
    public class CreateWorkflowCommand : IRequest<Result<WorkflowDto>>
    {
        public CreateWorkflowCommand(bool useMLTemplate = false)
        {
            UseMLTemplate = useMLTemplate;
        }

        public string WorkflowName { get; set; } = $"Workflow-created ({DateTime.Now})";
        public bool UseMLTemplate { get; }
    }
    public class CreateWorkflowHandler : IRequestHandler<CreateWorkflowCommand, Result<WorkflowDto>>
    {
        private readonly IRepository<Workflow> _workflowRepository;
        private readonly IMapper _mapper;
        private readonly IWorkflowTemplateService _workflowTemplateService;

        public CreateWorkflowHandler(IRepository<Workflow> workflowRepository, IMapper mapper, IWorkflowTemplateService workflowTemplateService)
        {
            _workflowRepository = workflowRepository;
            _mapper = mapper;
            _workflowTemplateService = workflowTemplateService;
        }
        public async Task<Result<WorkflowDto>> Handle(CreateWorkflowCommand request, CancellationToken cancellationToken)
        {
            var workflow = new Workflow()
            {
                Name = request.WorkflowName
            };

            if (request.UseMLTemplate)
            {
                var result = _workflowTemplateService.GetWorkflowSampleTemplate();
                if (!result.IsSuccess)
                {
                    return Result.Error(result.Errors.FirstOrDefault());
                }

                workflow.UpdateData(result.Value);
            }

            await _workflowRepository.AddAsync(workflow, cancellationToken);

            var mapped = _mapper.Map<Workflow, WorkflowDto>(workflow);
            return Result.Success(mapped);
        }
    }
}
