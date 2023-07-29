using AIaaS.Application.Features.Workflows.Notifications;
using AIaaS.Application.SignalR;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace AIaaS.WebAPI.SignalR
{
    public class WorkflowNodeValidateChangeHandler : INotificationHandler<WorkflowNodeValidateChangeNotification>
    {
        private readonly IHubContext<SignalRWorkflowRunHistoryHub, IWorkflowClient> _context;

        public WorkflowNodeValidateChangeHandler(IHubContext<SignalRWorkflowRunHistoryHub, IWorkflowClient> context)
        {
            _context = context;
        }

        public async Task Handle(WorkflowNodeValidateChangeNotification notification, CancellationToken cancellationToken)
        {
            //await _context.Clients.All.ReceiveWorkflowNodeValidateUpdate(notification.WorkflowNodeValidateChange);
        }
    }
}
