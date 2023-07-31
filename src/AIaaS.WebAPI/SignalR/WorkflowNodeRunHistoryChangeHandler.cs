using AIaaS.Application.Common.Models;
using AIaaS.Application.Features.Workflows.Notifications;
using AIaaS.Application.SignalR;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace AIaaS.WebAPI.SignalR
{
    public class WorkflowNodeRunHistoryChangeHandler : INotificationHandler<WorkflowNodeRunHistoryChangeNotification>
    {
        private readonly IHubContext<SignalRWorkflowRunHistoryHub, IWorkflowClient> _context;

        public WorkflowNodeRunHistoryChangeHandler(IHubContext<SignalRWorkflowRunHistoryHub, IWorkflowClient> context)
        {
            _context = context;
        }

        public async Task Handle(WorkflowNodeRunHistoryChangeNotification notification, CancellationToken cancellationToken)
        {
            var message = WebSocketMessage.CreateMessage(notification);
            await _context.Clients.All.ReceiveWorkflowUpdate(message);
        }
    }
}
