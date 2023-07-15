using AIaaS.Application.Common.Models;
using AIaaS.Application.Common.Models.Dtos;
using AIaaS.Application.Specifications.Workflows;
using AIaaS.Domain.Entities;
using AIaaS.Domain.Interfaces;
using Ardalis.Result;
using AutoMapper;
using MediatR;

namespace AIaaS.Application.Features.Workflows.Commands
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
        private readonly IRepository<Workflow> _workflowRepository;
        private readonly IMapper _mapper;

        public RenameWorkflowHandler(IRepository<Workflow> workflowRepository, IMapper mapper)
        {
            _workflowRepository = workflowRepository;
            _mapper = mapper;
        }

        public async Task<Result<WorkflowDto>> Handle(RenameWorkflowCommand request, CancellationToken cancellationToken)
        {
            var workflow = await _workflowRepository.FirstOrDefaultAsync(new WorkflowByIdSpec(request.RenameParameter.Id), cancellationToken);
            if (workflow is null) return Result.NotFound();

            workflow.Name = request.RenameParameter.Name;
            workflow.Description = request.RenameParameter.Description;

            await _workflowRepository.UpdateAsync(workflow, cancellationToken);

            var mapped = _mapper.Map<Workflow, WorkflowDto>(workflow);

            return Result.Success(mapped);
        }
    }
}
