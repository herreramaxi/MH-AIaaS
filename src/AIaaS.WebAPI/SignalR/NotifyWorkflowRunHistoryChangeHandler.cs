using AIaaS.Application.Common.Models;
using AIaaS.Application.Features.Workflows;
using AIaaS.Application.SignalR;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace AIaaS.WebAPI.SignalR
{
    public class NotifyWorkflowRunHistoryChangeHandler : IRequestHandler<NotifyWorkflowRunHistoryChangeRequest, WorkflowRunHistoryDto>
    {
        private readonly IHubContext<SignalRWorkflowRunHistoryHub, IWorkflowCLient> _context;
        private readonly IMapper _mapper;

        public NotifyWorkflowRunHistoryChangeHandler(IHubContext<SignalRWorkflowRunHistoryHub, IWorkflowCLient> context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<WorkflowRunHistoryDto> Handle(NotifyWorkflowRunHistoryChangeRequest request, CancellationToken cancellationToken)
        {
            var mapped = _mapper.Map<WorkflowRunHistoryDto>(request.WorkflowRunHistory);
            await _context.Clients.All.ReceiveWorkflowRunHistoryUpdate(mapped);

            return mapped;
        }
    }
}
