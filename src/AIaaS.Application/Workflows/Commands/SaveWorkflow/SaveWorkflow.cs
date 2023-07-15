
using AIaaS.Application.Common.Models;
using AIaaS.Application.Specifications;
using AIaaS.Domain.Entities;
using AIaaS.Domain.Interfaces;
using Ardalis.Result;
using AutoMapper;
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
        private readonly IRepository<Workflow> _workflowRepository;
        private readonly IMapper _mapper;

        public SaveWorkflowHandler(IRepository<Workflow> workflowRepository, IMapper mapper)
        {
            _workflowRepository = workflowRepository;
            _mapper = mapper;
        }
        public async Task<Result<WorkflowDto>> Handle(SaveWorkflowCommand request, CancellationToken cancellationToken)
        {
            var workflowDto = request.WorkflowSaveDto;
            var workflow = await _workflowRepository.FirstOrDefaultAsync(new WorkflowByIdSpec(workflowDto.Id), cancellationToken);
            if (workflow == null) return Result.NotFound();

            workflow.UpdateData(workflowDto.Root);
            await _workflowRepository.UpdateAsync(workflow, cancellationToken);

            var mapped = _mapper.Map<Workflow, WorkflowDto>(workflow);

            return Result.Success(mapped);
        }
    }
}
