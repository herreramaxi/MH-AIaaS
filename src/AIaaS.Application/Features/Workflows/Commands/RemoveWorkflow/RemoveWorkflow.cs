using AIaaS.Application.Specifications;
using AIaaS.Application.Specifications.Workflows;
using AIaaS.Domain.Entities;
using AIaaS.Domain.Interfaces;
using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIaaS.Application.Features.Workflows.Commands
{
    public class RemoveWorkflowCommand : IRequest<Result>
    {
        public RemoveWorkflowCommand(int workflowId)
        {
            WorkflowId = workflowId;
        }

        public int WorkflowId { get; }
    }

    public class RemoveWorkflowHandler : IRequestHandler<RemoveWorkflowCommand, Result>
    {
        private readonly IRepository<Workflow> _workflowRepository;
        private readonly IS3Service _s3Service;
        private readonly ILogger<RemoveWorkflowHandler> _logger;

        public RemoveWorkflowHandler(IRepository<Workflow> workflowRepository, IS3Service s3Service, ILogger<RemoveWorkflowHandler> logger)
        {
            _workflowRepository = workflowRepository;
            _s3Service = s3Service;
            _logger = logger;
        }
        public async Task<Result> Handle(RemoveWorkflowCommand request, CancellationToken cancellationToken)
        {
            var workflow = await _workflowRepository.FirstOrDefaultAsync(new WorkflowByIdWithWorkflowDataViewSpec(request.WorkflowId), cancellationToken);
            if (workflow is null)
                return Result.NotFound();

            foreach (var dataView in workflow.WorkflowDataViews)
            {
                var result = await _s3Service.DeleteFileAsync(dataView.S3Key);

                if (!result.IsSuccess)
                {
                    _logger.LogWarning($"Not able to delete WorkflowDataView from S3, key: {dataView.S3Key}");
                }
            }

            await _workflowRepository.DeleteAsync(workflow, cancellationToken);

            return Result.Success();
        }
    }
}
