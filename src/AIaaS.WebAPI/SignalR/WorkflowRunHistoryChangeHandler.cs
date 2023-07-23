using AIaaS.Application.Common.Models;
using AIaaS.Application.Features.Workflows;
using AIaaS.Application.SignalR;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace AIaaS.WebAPI.SignalR
{
    public class WorkflowRunHistoryChangeHandler : INotificationHandler<WorkflowRunHistoryChangeNotification>
    {
        private readonly IHubContext<SignalRWorkflowRunHistoryHub, IWorkflowCLient> _context;
        private readonly IMapper _mapper;

        public WorkflowRunHistoryChangeHandler(IHubContext<SignalRWorkflowRunHistoryHub, IWorkflowCLient> context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
     
        public async Task Handle(WorkflowRunHistoryChangeNotification notification, CancellationToken cancellationToken)
        {
            var mapped = _mapper.Map<WorkflowRunHistoryDto>(notification.WorkflowRunHistory);
            await _context.Clients.All.ReceiveWorkflowRunHistoryUpdate(mapped);
        }
    }
}
