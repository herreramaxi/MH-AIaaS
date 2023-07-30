using AIaaS.Application.Common.Models;
using AIaaS.Application.Interfaces;
using AIaaS.Application.Services;
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
        private readonly IDataViewService _dataViewService;

        public GetPreviewWorkflowHandler(IReadRepository<WorkflowDataView> readRepository, IDataViewService dataViewService)
        {
            _readRepository = readRepository;
            _dataViewService = dataViewService;
        }
        public async Task<Result<DataViewFilePreviewDto?>> Handle(GetPreviewWorkflowQuery request, CancellationToken cancellationToken)
        {
            var dataView = await _readRepository.FirstOrDefaultAsync(new WorkflowDataViewByIdSpec(request.WorkflowDataviewId), cancellationToken);
            if (dataView is null) return Result.NotFound();

            var preview = await _dataViewService.GetPreviewAsync(dataView);

            return Result.Success(preview);
        }
    }
}
