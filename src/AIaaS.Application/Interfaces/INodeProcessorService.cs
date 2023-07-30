using AIaaS.Application.Common.Models;
using AIaaS.Application.Common.Models.Dtos;
using Ardalis.Result;
using static AIaaS.Application.Services.NodeProcessorService;

namespace AIaaS.Application.Interfaces
{
    public interface INodeProcessorService
    {
        public event NodeStartProcessingHandler NodeStartProcessingEvent;
        public event NodeFinishProcessingHandler NodeFinishProcessingEvent;
        Task<Result<WorkflowDto>> Run(WorkflowDto workflowDto, WorkflowContext context, CancellationToken cancellationToken, bool ignoreNodeErrors = false);
    }
}
