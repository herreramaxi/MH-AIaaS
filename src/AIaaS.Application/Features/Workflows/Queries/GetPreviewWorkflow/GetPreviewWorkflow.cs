using AIaaS.Application.Common.Models;
using AIaaS.Application.Specifications.Workflows;
using AIaaS.Domain.Entities;
using AIaaS.Domain.Interfaces;
using AIaaS.WebAPI.ExtensionMethods;
using Ardalis.Result;
using MediatR;

namespace AIaaS.Application.Features.Workflows.Queries
{
    public class GetPreviewWorkflowQuery : IRequest<Result<DataViewFilePreviewDto?>>
    {
        public GetPreviewWorkflowQuery(int workflowDataviewId)
        {
            WorkflowDataviewId = workflowDataviewId;
        }

        public int WorkflowDataviewId { get; }
    }

    public class GetPreviewWorkflowHandler : IRequestHandler<GetPreviewWorkflowQuery, Result<DataViewFilePreviewDto?>>
    {
        private readonly IReadRepository<WorkflowDataView> _readRepository;

        public GetPreviewWorkflowHandler(IReadRepository<WorkflowDataView> readRepository)
        {
            _readRepository = readRepository;
        }
        public async Task<Result<DataViewFilePreviewDto?>> Handle(GetPreviewWorkflowQuery request, CancellationToken cancellationToken)
        {
            var dataView = await _readRepository.FirstOrDefaultAsync(new WorkflowDataViewByIdSpec(request.WorkflowDataviewId), cancellationToken);
            if (dataView?.Data is null) return Result.NotFound();

            var dataPreview = dataView.Data.GetPreview();

            return Result.Success(dataPreview);
        }
    }
}
